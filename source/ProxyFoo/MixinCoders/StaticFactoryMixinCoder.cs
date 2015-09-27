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
using ProxyFoo.Mixins;

namespace ProxyFoo.MixinCoders
{
    public class StaticFactoryMixinCoder : MixinCoderBase
    {
        TypeBuilder _tb;
        MethodBuilder _factoryDelegateMethod;
        ConstructorInfo _ctor;
        Type[] _ctorArgTypes;

        public StaticFactoryMixinCoder(ConstructorInfo ctor)
        {
            _ctor = ctor;
        }

        public override void SetupCtor(IProxyCtorBuilder pcb) {}

        public override void Generate(IProxyCodeBuilder pcb)
        {
            _tb = (TypeBuilder)pcb.SelfType;
            if (_ctor==null)
            {
                _ctor = pcb.Ctor;
                _ctorArgTypes = pcb.CtorArgs.ToArray();
            }
            else
            {
                _ctorArgTypes = _ctor.GetParameters().Select(a => a.ParameterType).ToArray();
            }
            GenFactoryDelegateMethod();
            GenFactoryMethod();
        }

        void GenFactoryDelegateMethod()
        {
            // This generates the method that will be wrapped by the delegate in the GetFactory method
            _factoryDelegateMethod = _tb.DefineMethod(
                "FactoryDelegateMethod",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                typeof(object),
                _ctorArgTypes.Select(a => typeof(object)).ToArray());

            var gen = _factoryDelegateMethod.GetILGenerator();

            ushort index = 0;
            foreach (var arg in _ctorArgTypes)
            {
                gen.EmitBestLdArg(index);
                gen.Emit(OpCodes.Castclass, arg);
                ++index;
            }

            gen.Emit(OpCodes.Newobj, _ctor);
            gen.Emit(OpCodes.Ret);
        }

        void GenFactoryMethod()
        {
            var factoryFuncType = GetFactoryFuncType();
            var method = _tb.DefineMethod(
                StaticFactoryMixin.CreateFactoryMethodName,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
                factoryFuncType,
                null);

            var gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ldftn, _factoryDelegateMethod);
            gen.Emit(OpCodes.Newobj, factoryFuncType.GetConstructors().First());
            gen.Emit(OpCodes.Ret);
        }

        Type GetFactoryFuncType()
        {
            switch (_ctorArgTypes.Length)
            {
                case 0:
                    return typeof(Func<object>);
                case 1:
                    return typeof(Func<object, object>);
                case 2:
                    return typeof(Func<object, object, object>);
                case 3:
                    return typeof(Func<object, object, object, object>);
                default:
                    throw new Exception("Factory Func type not available.");
            }
        }
    }
}