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
    public class ByRefConversionSample
    {
        public static void DoSomething(out int value)
        {
            value = 42;
        }

        public static void DoSomething(out long value)
        {
            int intVal;
            DoSomething(out intVal);
            value = intVal;
        }

        public struct SampleStruct
        {
            public int Value;
        }

        public struct SampleStructB
        {
            public SampleStructB(int value)
            {
                Value = value;
            }

            public int Value;

            public static implicit operator SampleStructB(SampleStruct value)
            {
                return new SampleStructB(value.Value);
            }
        }

        public static void DoSomethingA(out SampleStruct value)
        {
            value = new SampleStruct();
        }

        public static void DoSomethingB(out SampleStruct value)
        {
            DoSomethingA(out value);
        }

        public static void DoSomethingC(out SampleStructB value)
        {
            SampleStruct local;
            DoSomethingA(out local);
            value = local;
        }

        public static void StoreToRef(decimal value, out decimal outValue)
        {
            outValue = value;
        }
    }
}