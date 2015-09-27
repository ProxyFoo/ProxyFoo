#region Apache License Notice

// Copyright © 2015, Silverlake Software LLC
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

namespace SampleCode
{
    public class DynamicMethodSample
    {
        class Sample
        {
            void DoIt(int a)
            {
                Console.WriteLine(a);
            }
        }

        public static Delegate CreateDelegate(MethodInfo methodInfo)
        {
            var declaringType = methodInfo.DeclaringType;
            var dm = new DynamicMethod("test", null, new[] {declaringType, typeof(int)}, declaringType, false);
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, methodInfo);
            gen.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(DoItDelegate));
        }

        delegate void DoItDelegate(Sample sample, int a);

        public static void CallDoIt()
        {
            var methodInfo = typeof(Sample).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(mi => mi.Name=="DoIt");

            var action = (DoItDelegate)CreateDelegate(methodInfo);
            action(new Sample(), 1);

            var action2 = (DoItDelegate)Delegate.CreateDelegate(typeof(DoItDelegate), methodInfo);
            action2(new Sample(), 2);
        }
    }
}