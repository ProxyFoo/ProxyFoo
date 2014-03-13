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
using System.Reflection;
using System.Reflection.Emit;

namespace ProxyFoo.Core.Bindings
{
    public class ImplicitUserConversionValueBinding : DuckValueBindingOption
    {
        readonly MethodInfo _method;

        public static DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            Type coreFromType = Nullable.GetUnderlyingType(fromType);
            Type coreToType = Nullable.GetUnderlyingType(toType);

            // Cannot convert a nullable value type to a non-nullable value type
            if (coreFromType!=null && coreToType==null && toType.IsValueType)
                return null;

            Type finalFromType = coreFromType ?? fromType;
            Type finalToType = coreToType ?? toType;

            var method = finalFromType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new[] {finalFromType}, null);
            if (method==null)
            {
                method = finalToType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new[] {finalFromType}, null);
                if (method==null)
                    return null;
            }

            DuckValueBindingOption userConvBinding = new ImplicitUserConversionValueBinding(method);
            return coreFromType!=null ? new ImplicitNullableValueBinding(true, fromType, toType, coreToType, userConvBinding) : userConvBinding;
        }

        ImplicitUserConversionValueBinding(MethodInfo method)
        {
            _method = method;
        }

        public override bool Bindable
        {
            get { return true; }
        }

        public override int Score
        {
            get { return 1; }
        }

        public override void GenerateConversion(IProxyModuleCoderAccess proxyModule, ILGenerator gen)
        {
            gen.Emit(OpCodes.Call, _method);
        }
    }
}