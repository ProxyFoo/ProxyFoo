#region Apache License Notice

// Copyright © 2012, Silverlake Software LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core.Foo;

namespace ProxyFoo.Core
{
    public class ProxyClassCoder : IProxyCoderContext
    {
        readonly ProxyClassDescriptor _pcd;
        readonly IProxyModuleCoderAccess _pm;
        readonly ModuleBuilder _mb;
        TypeBuilder _tb;
        IFooTypeBuilder _ftb;
        readonly List<MixinCoderContext> _mixinCoderContexts = new List<MixinCoderContext>();
        readonly List<IProxyCtorCoder> _ctorCoders = new List<IProxyCtorCoder>();
        readonly List<FieldInfo> _fields = new List<FieldInfo>();
        ConstructorBuilder _ctor;

        protected internal ProxyClassCoder(IProxyModuleCoderAccess pm, ProxyClassDescriptor pcd)
        {
            _pm = pm;
            _mb = pm.ModuleBuilder;
            _pcd = pcd;
        }

        public ProxyClassDescriptor Descriptor
        {
            get { return _pcd; }
        }

        public IProxyModuleCoderAccess ProxyModule
        {
            get { return _pm; }
        }

        public ModuleBuilder ModuleBuilder
        {
            get { return _mb; }
        }

        public IEnumerable<IMixinCoderContext> MixinCoderContexts
        {
            get { return _mixinCoderContexts; }
        }

        public virtual Type Generate(Action<IFooTypeBuilder> whenTypeDefined)
        {
            string typeName = _pm.AssemblyName + ".Proxy_" + Guid.NewGuid().ToString("N");

            _tb = _mb.DefineType(typeName, TypeAttributes.Class, _pcd.BaseClassType,
                _pcd.Mixins.SelectMany(m => m.Subjects).Select(s => s.Type).ToArray());
            _ftb = new FooTypeFromTypeBuilder(_tb);
            whenTypeDefined(_ftb);

            CreateMixinCoderContexts();
            _mixinCoderContexts.ForEach(a => a.Start());
            _mixinCoderContexts.ForEach(a => a.SetupCtor());
            GenerateConstructors(_pcd.BaseClassType);
            _mixinCoderContexts.ForEach(a => a.Generate());
            _mixinCoderContexts.ForEach(a => a.CreateSubjectCoderContexts());
            _mixinCoderContexts.ForEach(a => a.GenerateSubjects());

            return _tb.CreateType();
        }

        protected virtual void CreateMixinCoderContexts()
        {
            int i = 0;
            foreach (var mixin in _pcd.Mixins)
            {
                _mixinCoderContexts.Add(new MixinCoderContext(this, i, mixin));
                ++i;
            }
        }

        protected virtual void GenerateConstructors(Type baseType)
        {
            foreach (var ctor in baseType.GetConstructors())
                GenerateConstructor(ctor);
        }

        protected virtual void GenerateConstructor(ConstructorInfo baseCtor)
        {
            var baseParameters = baseCtor.GetParameters();
            _ctor = _ftb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                baseParameters.Select(p => p.ParameterType)
                    .Concat(_ctorCoders.SelectMany(c => c.Args)).ToArray());

            var gen = _ctor.GetILGenerator();

            // Call to base constructor using matching args
            gen.Emit(OpCodes.Ldarg_0);
            ushort index = 1;
            for (; index <= baseParameters.Length; ++index)
                gen.EmitBestLdArg(index);
            gen.Emit(OpCodes.Call, baseCtor);

            // Process additional args for the proxy
            foreach (var ctorCoder in _ctorCoders)
            {
                ctorCoder.Start(gen);
                foreach (var arg in ctorCoder.Args)
                {
                    ctorCoder.ProcessArg(gen, index);
                    ++index;
                }
                ctorCoder.Complete(gen);
            }

            gen.Emit(OpCodes.Ret);
        }

        internal void GenerateMethods(Type type, ISubjectCoder sc)
        {
            sc.Start(this);

            var definedProperties = new Dictionary<PropertyInfo, PropertyBuilder>();
            foreach (var subjectMethod in SubjectMethod.GetAllForType(type))
            {
                var pi = subjectMethod.PropertyInfo;
                var mi = subjectMethod.MethodInfo;
                if (pi!=null)
                {
                    // If this is the first method in the property, define the property
                    PropertyBuilder property;
                    if (!definedProperties.TryGetValue(pi, out property))
                    {
                        property = DefineProperty(pi);
                        definedProperties.Add(pi, property);
                    }

                    // Define the method and attach it to the property
                    var method = DefineMethod(mi);
                    sc.GenerateMethod(pi, mi, method.GetILGenerator());
                    if (pi.GetGetMethod()==mi)
                        property.SetGetMethod(method);
                    else
                        property.SetSetMethod(method);
                }
                else
                {
                    var method = DefineMethod(mi);
                    sc.GenerateMethod(null, mi, method.GetILGenerator());
                }
            }
        }

        PropertyBuilder DefineProperty(PropertyInfo fromProperty)
        {
            var name = fromProperty.Name;
            var parTypes = fromProperty.GetIndexParameters().Select(p => p.ParameterType).ToArray();
            var existingProperty = _ftb.GetProperty(name, fromProperty.PropertyType, parTypes);
            if (existingProperty!=null)
                name = GetPrefixForPrivateImplementation(fromProperty.DeclaringType) + name;
            return _ftb.DefineProperty(name, PropertyAttributes.None, fromProperty.PropertyType, parTypes);
        }

        MethodBuilder DefineMethod(MethodInfo fromMethod)
        {
            var name = fromMethod.Name;
            var parTypes = fromMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            var existingMethod = _ftb.GetMethod(name, parTypes);

            var methodAttrs = (existingMethod==null ? MethodAttributes.Public : MethodAttributes.Private) |
                              MethodAttributes.Virtual | MethodAttributes.Final |
                              MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            if (existingMethod!=null)
                name = GetPrefixForPrivateImplementation(fromMethod.DeclaringType) + name;
            var method = _ftb.DefineMethod(name, methodAttrs, fromMethod.ReturnType, parTypes);

            var genArgs = fromMethod.GetGenericArguments();
            if (genArgs.Length > 0)
            {
                var builders = method.DefineGenericParameters(genArgs.Select(a => a.Name).ToArray());
                for (int i = 0; i < genArgs.Length; ++i)
                {
                    var genArg = genArgs[i];
                    var builder = builders[i];

                    builder.SetGenericParameterAttributes(genArg.GenericParameterAttributes());
                    var constraints = genArg.GetGenericParameterConstraints();
                    var baseTypeConstraint = constraints.SingleOrDefault(a => a.IsClass());
                    if (baseTypeConstraint!=null)
                        builder.SetBaseTypeConstraint(baseTypeConstraint);
                    builder.SetInterfaceConstraints(constraints.Where(a => !a.IsClass()).ToArray());
                }
            }

            if (existingMethod!=null)
                _tb.DefineMethodOverride(method, fromMethod);

            return method;
        }

        static string GetPrefixForPrivateImplementation(Type type)
        {
            return (type.IsGenericType() ? type.GetGenericTypeDefinition().FullName : type.FullName) + ".";
        }

        class MixinCoderContext : IMixinCoderContext
        {
            readonly ProxyClassCoder _proxyCoder;
            readonly int _index;
            readonly IMixinDescriptor _mixin;
            readonly IMixinCoder _mixinCoder;
            List<SubjectCoderContext> _subjectCoderStates;
            ProxyCodeBuilder _pcb;

            public MixinCoderContext(ProxyClassCoder proxyCoder, int index, IMixinDescriptor mixin)
            {
                _proxyCoder = proxyCoder;
                _mixin = mixin;
                _index = index;
                _mixinCoder = mixin.CreateCoder();
            }

            public IProxyCoderContext ProxyCoderContext
            {
                get { return _proxyCoder; }
            }

            public IEnumerable<ISubjectCoderContext> SubjectCoderContexts
            {
                get
                {
                    if (_subjectCoderStates==null)
                        throw new InvalidOperationException("Cannot request SubjectCoderContexts at this stage in proxy class generation.");
                    return _subjectCoderStates;
                }
            }

            internal void Start()
            {
                _pcb = new ProxyCodeBuilder(_proxyCoder, _index);
                _mixinCoder.Start(_pcb);
            }

            internal void SetupCtor()
            {
                var pcb = new ProxyCtorBuilder(_proxyCoder, _index);
                _mixinCoder.SetupCtor(pcb);
            }

            internal void Generate()
            {
                _pcb = new ProxyCodeBuilder(_proxyCoder, _index);
                _mixinCoder.Generate(_pcb);
            }

            internal void CreateSubjectCoderContexts()
            {
                _subjectCoderStates = _mixin.Subjects.Select(a => new SubjectCoderContext(_pcb, _mixinCoder, a)).ToList();
            }

            internal void GenerateSubjects()
            {
                foreach (var scc in _subjectCoderStates)
                    scc.GenerateMethods(_proxyCoder);
            }
        }

        class ProxyCtorBuilder : IProxyCtorBuilder
        {
            readonly ProxyClassCoder _proxyCoder;
            readonly int _index;

            public ProxyCtorBuilder(ProxyClassCoder proxyCoder, int index)
            {
                _proxyCoder = proxyCoder;
                _index = index;
            }

            public FieldInfo AddField(Type type, string name)
            {
                name += "_" + _index;
                var field = _proxyCoder._tb.DefineField(name, type, FieldAttributes.Private);
                _proxyCoder._fields.Add(field);
                return field;
            }

            public void SetCtorCoder(IProxyCtorCoder ctorCoder)
            {
                _proxyCoder._ctorCoders.Add(ctorCoder);
            }
        }

        class ProxyCodeBuilder : IProxyCodeBuilder
        {
            readonly ProxyClassCoder _proxyCoder;
            readonly int _index;

            public ProxyCodeBuilder(ProxyClassCoder proxyCoder, int index)
            {
                _proxyCoder = proxyCoder;
                _index = index;
            }

            public IProxyCoderContext ProxyCoderContext
            {
                get { return _proxyCoder; }
            }

            public ConstructorInfo Ctor
            {
                get { return _proxyCoder._ctor; }
            }

            public IEnumerable<Type> CtorArgs
            {
                get { return _proxyCoder._ctorCoders.SelectMany(c => c.Args); }
            }
#if FEATURE_LEGACYREFLECTION
            public Type SelfType
            {
                get { return _proxyCoder._tb; }
            }
#else
            public TypeInfo SelfType
            {
                get { return _proxyCoder._tb; }
            }
#endif


            public IFooTypeBuilder SelfTypeBuilder
            {
                get { return _proxyCoder._ftb; }
            }

            public ILGenerator DefineStaticCtor()
            {
                return _proxyCoder._tb.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, null).GetILGenerator();
            }

            public FieldInfo AddStaticField(string name, Type type)
            {
                if (_index > 0)
                    throw new Exception("Static fields are only supported on the first mixin.");
                var field = _proxyCoder._ftb.DefineField(name, type, FieldAttributes.Static | FieldAttributes.Public);
                _proxyCoder._fields.Add(field);
                return field;
            }

            public FieldInfo AddField(string name, Type type)
            {
                name += "_" + _index;

                var field = _proxyCoder._tb.DefineField(name, type, FieldAttributes.Private);
                _proxyCoder._fields.Add(field);
                return field;
            }
        }

        class SubjectCoderContext : ISubjectCoderContext
        {
            readonly ProxyCodeBuilder _pcb;
            readonly ISubjectDescriptor _subject;
            readonly ISubjectCoder _subjectCoder;

            public SubjectCoderContext(ProxyCodeBuilder pcb, IMixinCoder mixinCoder, ISubjectDescriptor subject)
            {
                _pcb = pcb;
                _subject = subject;
                _subjectCoder = subject.CreateCoder(mixinCoder, pcb);
            }

            Type ISubjectCoderContext.SubjectType
            {
                get { return _subject.Type; }
            }

            T ISubjectCoderContext.GetPerSubjectCoder<T>()
            {
                var factory = _subject as IPerSubjectCoderFactory<T>;
                return factory==null ? null : factory.CreateCoder(_pcb);
            }

            internal void GenerateMethods(ProxyClassCoder pcc)
            {
                pcc.GenerateMethods(_subject.Type, _subjectCoder);
            }
        }
    }
}