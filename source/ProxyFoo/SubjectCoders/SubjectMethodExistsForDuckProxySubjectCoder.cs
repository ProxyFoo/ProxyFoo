#region Apache License Notice

// Copyright © 2014, Silverlake Software LLC
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.Bindings;
using ProxyFoo.Core.Foo;
using ProxyFoo.Core.SubjectTypes;
using ProxyFoo.Mixins;

namespace ProxyFoo.SubjectCoders
{
    class SubjectMethodExistsForDuckProxySubjectCoder : ISubjectCoder
    {
        readonly Type _methodExistsSubjectType;
        readonly Type _realSubjectType;
        readonly IFooType _methodIndexProxyType;
        readonly IFooTypeBuilder _ftb;
        readonly MethodInfo _smiMethod;

        public SubjectMethodExistsForDuckProxySubjectCoder(IProxyCodeBuilder pcb, Type methodExistsSubjectType, Type realSubjectType)
        {
            _methodExistsSubjectType = methodExistsSubjectType;
            _realSubjectType = realSubjectType;
            var pcd = MethodIndexFactory.GetProxyClassDescriptorForSubjectType(methodExistsSubjectType);
            _methodIndexProxyType = pcb.ProxyCoderContext.ProxyModule.GetTypeFromProxyClassDescriptor(pcd);
            _ftb = pcb.SelfTypeBuilder;
            _smiMethod = GenerateStaticFromMethodIndex();
        }

        MethodInfo GenerateStaticFromMethodIndex()
        {
            var method = _ftb.DefineMethod(
                "_GetSmeResult",
                MethodAttributes.Static | MethodAttributes.Private,
                typeof(bool),
                new[] {typeof(int)});
            var gen = method.GetILGenerator();
            var methods = SubjectMethod.GetAllForType(_methodExistsSubjectType).ToArray();
            if (methods.Length==1)
            {
                PutMethodExistsOnStack(methods[0].MethodInfo, gen);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                var labels = methods.Select(_ => gen.DefineLabel()).ToArray();
                var exitLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Switch, labels);
                int index = 0;
                foreach (var m in methods)
                {
                    gen.MarkLabel(labels[index]);
                    PutMethodExistsOnStack(m.MethodInfo, gen);
                    gen.Emit(OpCodes.Br, exitLabel);
                    ++index;
                }
                gen.MarkLabel(exitLabel);
            }
            gen.Emit(OpCodes.Ret);
            return method;
        }

        public void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var parameters = mi.GetParameters();
            if (parameters.Length!=1)
                ThrowUnrecognizedMethod();

            Type parameterType = parameters[0].ParameterType;
            if (parameterType==typeof(int))
                GenerateFromMethodIndex(gen);
            else if (parameterType.GetGenericTypeDefinition()==typeof(Action<>))
                GenerateFromAction(gen);
            else if (parameterType.GetGenericTypeDefinition()==typeof(Func<,>))
                GenerateFromFunc(gen, mi.GetGenericArguments()[0]);
            else
                ThrowUnrecognizedMethod();
        }

        static void ThrowUnrecognizedMethod()
        {
            throw new Exception("Unrecognized method on ISubjectMethodExists");
        }

        void GenerateFromMethodIndex(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, _smiMethod);
            gen.Emit(OpCodes.Ret);
        }

        void GenerateFromAction(ILGenerator gen)
        {
            var cmi = gen.DeclareLocal(_methodIndexProxyType.AsType());
            StaticInstanceMixin.PushInstanceOnStackFor(_methodIndexProxyType, gen);
            gen.Emit(OpCodes.Stloc, cmi);

            //int DoesMethodExist(Action<T> exemplar);
            gen.Emit(OpCodes.Ldarg_1); // push exemplar
            gen.Emit(OpCodes.Ldloc, cmi); // push the ISubjectType
            gen.Emit(OpCodes.Callvirt, typeof(Action<>).MakeGenericType(_methodExistsSubjectType).GetMethod("Invoke"));
            gen.Emit(OpCodes.Ldloc, cmi); // push the IComputeMethodExistsResult
            gen.Emit(OpCodes.Callvirt, typeof(IComputeMethodIndexResult).GetProperty("MethodIndex").GetGetMethod());
            gen.Emit(OpCodes.Call, _smiMethod);
            gen.Emit(OpCodes.Ret);
        }

        void GenerateFromFunc(ILGenerator gen, Type tOut)
        {
            var cmi = gen.DeclareLocal(_methodIndexProxyType.AsType());
            StaticInstanceMixin.PushInstanceOnStackFor(_methodIndexProxyType, gen);
            gen.Emit(OpCodes.Stloc, cmi);

            //int DoesMethodExist(Func<T,TOut> exemplar);
            gen.Emit(OpCodes.Ldarg_1); // push exemplar
            gen.Emit(OpCodes.Ldloc, cmi); // push the ISubjectType
            gen.Emit(OpCodes.Callvirt, typeof(Func<,>).MakeGenericType(_methodExistsSubjectType, tOut).GetMethod("Invoke"));
            gen.Emit(OpCodes.Pop); // pop the return value
            gen.Emit(OpCodes.Ldloc, cmi); // push the IComputeMethodExistsResult
            gen.Emit(OpCodes.Callvirt, typeof(IComputeMethodIndexResult).GetProperty("MethodIndex").GetGetMethod());
            gen.Emit(OpCodes.Call, _smiMethod);
            gen.Emit(OpCodes.Ret);
        }

        void PutMethodExistsOnStack(MethodInfo mi, ILGenerator gen)
        {
            var matches = from cmi in _realSubjectType.GetMethods()
                          where cmi.Name==mi.Name
                          let mbo = DuckMethodBindingOption.Get(mi, cmi)
                          where mbo.Bindable
                          orderby mbo.Score descending
                          select mbo;
            var bestMatch = matches.FirstOrDefault();
            gen.Emit(bestMatch!=null ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
    }
}