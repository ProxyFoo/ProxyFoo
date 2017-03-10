#region Apache License Notice

// Copyright © 2016, Silverlake Software LLC
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
using NUnit.Common;
using NUnitLite;
using ProxyFoo.Tests;
using ProxyFoo.ExtensionApi;

namespace ProxyFooConsole_Package
{
    public class Program
    {
        public class Sample
        {
            public int GetAnswer()
            {
                return 42;
            }
        }

        public interface ISample
        {
            int GetAnswer();
        }

        public static void Main(string[] args)
        {
            if (args.Length!=1)
            {
                Console.WriteLine("Invalid argument.");
                return;
            }

            switch (args[0])
            {
                case "tests":
                    ExecuteTests(new[] { "--noresult" });
                    break;
                case "sample":
                    ExecuteSample();
                    break;
            }
        }

        public static void ExecuteTests(string[] args)
        {
            new AutoRun(typeof(ProxyFooTestsBase).GetTypeInfo().Assembly).Execute(args);
        }

        public static void ExecuteSample()
        {
            object target = new Sample();
            Console.WriteLine("ProxyFoo result = {0}", target.Duck<ISample>().GetAnswer());
        }
    }
}