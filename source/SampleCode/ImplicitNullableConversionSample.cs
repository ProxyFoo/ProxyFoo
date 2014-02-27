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

namespace SampleCode
{
    public class ImplicitNullableConversionSample
    {
        public static void NonNullableToNullableIdentity()
        {
            MethodIntArg(42);
        }

        public static void NullableToNullableIdentity()
        {
            int? value = 42;
            MethodIntArg(value);
        }

        public static void NonNullableToNullableImplicitNumeric()
        {
            MethodLongArg((int)42);
        }

        public static void NullableToNullableImplicitNumeric()
        {
            int? value = 42;
            MethodLongArg(value);
        }

        public static void MethodIntArg(int? value) {}

        public static void MethodLongArg(long? value) {}
    }
}