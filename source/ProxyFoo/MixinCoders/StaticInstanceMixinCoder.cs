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
using ProxyFoo.Core.Foo;
using ProxyFoo.Mixins;

namespace ProxyFoo.MixinCoders
{
    public class StaticInstanceMixinCoder : MixinCoderBase
    {
        readonly StaticInstanceOptions _options;

        public StaticInstanceMixinCoder(StaticInstanceOptions options)
        {
            _options = options;
        }

        public override void SetupCtor(IProxyCtorBuilder pcb) {}

        public override void Generate(IProxyCodeBuilder pcb)
        {
            if (pcb.CtorArgs.Any())
                throw new InvalidOperationException("Cannot use StaticInstanceMixin when there are constructor arguments");

            var ftb = pcb.SelfTypeBuilder;

            var funcField = ftb.DefineField(
                StaticInstanceMixin.FuncInstanceFieldName,
                typeof(Func<object>),
                FieldAttributes.Static | FieldAttributes.Public);

            var field = ftb.DefineField(StaticInstanceMixin.InstanceFieldName, ftb.AsType(), FieldAttributes.Static | FieldAttributes.Private);
            if (_options.HasFlag(StaticInstanceOptions.ThreadStatic))
            {
                // ReSharper disable once AssignNullToNotNullAttribute  
                field.SetCustomAttribute(new CustomAttributeBuilder(typeof(ThreadStaticAttribute).GetConstructor(Type.EmptyTypes), new object[] {}));
            }

            var property = ftb.DefineProperty(StaticInstanceMixin.InstancePropertyName, PropertyAttributes.None, ftb.AsType(), Type.EmptyTypes);
            var getMethod = ftb.DefineMethod(
                StaticInstanceMixin.InstanceGetMethodName,
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                ftb.AsType(),
                Type.EmptyTypes);
            property.SetGetMethod(getMethod);

            // Generate get method body for property
            var gen = getMethod.GetILGenerator();
            var doneLabel = gen.DefineLabel();
            gen.Emit(OpCodes.Ldsfld, field); // push _i;
            gen.Emit(OpCodes.Dup); // push s0
            gen.Emit(OpCodes.Brtrue_S, doneLabel); // if [s0]!=null then goto doneLabel
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Newobj, pcb.Ctor); // new proxy object
            gen.Emit(OpCodes.Dup); // push s0
            gen.Emit(OpCodes.Stsfld, field); // _i = [s0]
            gen.MarkLabel(doneLabel);
            gen.Emit(OpCodes.Ret);

            GenerateStaticCtor(ftb, funcField, getMethod);
        }

        public void GenerateStaticCtor(IFooTypeBuilder ftb, FieldInfo funcField, MethodInfo getMethod)
        {
            var gen = ftb.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, null).GetILGenerator();
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ldftn, getMethod);
            gen.Emit(OpCodes.Newobj, typeof(Func<object>).GetConstructors().First());
            gen.Emit(OpCodes.Stsfld, funcField);
            gen.Emit(OpCodes.Ret);
        }
    }
}