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
        static readonly ConstructorInfo ObjectCtor;
        readonly ProxyClassDescriptor _pcd;
        readonly IProxyModuleCoderAccess _pm;
        readonly ModuleBuilder _mb;
        TypeBuilder _tb;
        IFooTypeBuilder _ftb;
        readonly List<MixinCoderContext> _mixinCoderContexts = new List<MixinCoderContext>();
        readonly List<ConstructorArgInfo> _constructorArgs = new List<ConstructorArgInfo>();
        readonly List<FieldInfo> _fields = new List<FieldInfo>();
        ConstructorBuilder _ctor;

        static ProxyClassCoder()
        {
            ObjectCtor = typeof(object).GetConstructor(Type.EmptyTypes);
        }

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
            _mixinCoderContexts.ForEach(a => a.SetupCtor());
            GenerateConstructor();
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

        protected virtual void GenerateConstructor()
        {
            _ctor = _ftb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                _constructorArgs.Select(a => a.Type).ToArray());

            var gen = _ctor.GetILGenerator();

            // Initialize this or it fails PE verification (still runs..)
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, ObjectCtor);

            ushort index = 1;
            foreach (var arg in _constructorArgs)
            {
                arg.ProcessArgGenAction(gen, index);
                ++index;
            }

            gen.Emit(OpCodes.Ret);
        }

        internal void GenerateMethods(Type type, ISubjectCoder sc)
        {
            var completed = new HashSet<MethodInfo>();

            foreach (var pi in type.GetProperties())
            {
                var property = DefineProperty(pi);

                if (pi.CanRead)
                {
                    var miGet = pi.GetGetMethod();
                    var newGetMethod = DefineMethod(miGet);
                    sc.GenerateMethod(pi, miGet, newGetMethod.GetILGenerator());
                    completed.Add(miGet);
                    property.SetGetMethod(newGetMethod);
                }

                if (pi.CanWrite)
                {
                    var miSet = pi.GetSetMethod();
                    var newSetMethod = DefineMethod(miSet);
                    sc.GenerateMethod(pi, miSet, newSetMethod.GetILGenerator());
                    completed.Add(miSet);
                    property.SetSetMethod(newSetMethod);
                }
            }

            foreach (var mi in type.GetMethods().Where(mi => !completed.Contains(mi)))
            {
                var method = DefineMethod(mi);
                sc.GenerateMethod(null, mi, method.GetILGenerator());
            }
        }

        PropertyBuilder DefineProperty(PropertyInfo fromProperty)
        {
            var name = fromProperty.Name;
            var parTypes = fromProperty.GetIndexParameters().Select(p => p.ParameterType).ToArray();
            var existingProperty = _ftb.GetProperty(name, parTypes);
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

                    builder.SetGenericParameterAttributes(genArg.GenericParameterAttributes);
                    var constraints = genArg.GetGenericParameterConstraints();
                    var baseTypeConstraint = constraints.SingleOrDefault(a => a.IsClass);
                    if (baseTypeConstraint!=null)
                        builder.SetBaseTypeConstraint(baseTypeConstraint);
                    builder.SetInterfaceConstraints(constraints.Where(a => !a.IsClass).ToArray());
                }
            }

            if (existingMethod!=null)
                _tb.DefineMethodOverride(method, fromMethod);

            return method;
        }

        static string GetPrefixForPrivateImplementation(Type type)
        {
            return (type.IsGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName) + ".";
        }

        class ConstructorArgInfo
        {
            internal Type Type { get; set; }
            internal Action<ILGenerator, ushort> ProcessArgGenAction { get; set; }
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
                _mixinCoder = mixin.CreateCoder();
                _index = index;
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

            public void AddArg(Type type, Action<ILGenerator, ushort> processArgGenAction)
            {
                _proxyCoder._constructorArgs.Add(new ConstructorArgInfo() {Type = type, ProcessArgGenAction = processArgGenAction});
            }

            public FieldInfo AddField(Type type, string name)
            {
                name += "_" + _index;
                var field = _proxyCoder._tb.DefineField(name, type, FieldAttributes.Private);
                _proxyCoder._fields.Add(field);
                return field;
            }

            public FieldInfo AddArgWithBackingField(Type type, string name)
            {
                var field = AddField(type, name);
                AddArg(type, (gen, i) => InitBackingField(gen, i, field));
                return field;
            }

            void InitBackingField(ILGenerator gen, ushort argIndex, FieldInfo field)
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.EmitBestLdArg(argIndex);
                gen.Emit(OpCodes.Stfld, field);
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
                get { return _proxyCoder._constructorArgs.Select(a => a.Type); }
            }

            public Type SelfType
            {
                get { return _proxyCoder._tb; }
            }

            public ILGenerator DefineStaticCtor()
            {
                return _proxyCoder._tb.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, null).GetILGenerator();
            }

            public FieldInfo AddStaticField(string name, Type type)
            {
                if (_index > 0)
                    throw new Exception("Static fields are not supported on multiple mixin proxies.");
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