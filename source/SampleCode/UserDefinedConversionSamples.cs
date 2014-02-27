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
    public class UserDefinedConversionSamples
    {
        public class UserClass
        {
            int _value;

            public UserClass(int value)
            {
                _value = value;
            }

            public static implicit operator UserClass(int value)
            {
                return new UserClass(value);
            }
        }

        public static void SampleConversionClass()
        {
            UserClass value = 4;
        }

        public static void SampleConversionClass2()
        {
            int? intVal = 4;
            UserClass value = intVal;
        }

        public static void SampleConversionClass3()
        {
            short? intVal = 4;
            UserClass value = intVal;
        }

        public struct UserStruct
        {
            int _value;

            public UserStruct(int value)
            {
                _value = value;
            }

            public static implicit operator UserStruct(int value)
            {
                return new UserStruct((int)value);
            }
        }

        public static void SampleConversionStruct()
        {
            UserStruct value = 5;
        }

        public static void SampleConversionStruct2()
        {
            int? intVal = 5;
            UserStruct? value = intVal;
        }
    }
}