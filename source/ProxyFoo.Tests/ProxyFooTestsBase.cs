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
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace ProxyFoo.Tests
{
    /// <summary>
    /// This class is a base TextFixture for tests that utilize dynamic code generation.  It contains the common code
    /// to save and verify the generated code.  This can be useful during debugging the tests and provides the additional
    /// confirmation that the generated code is verifiable.
    /// </summary>
    [TestFixture]
    public abstract class ProxyFooTestsBase
    {
        static readonly string PeVerifyPath;

        static ProxyFooTestsBase()
        {
            PeVerifyPath = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("peverify.exe", TargetDotNetFrameworkVersion.Version40);
        }

        [SetUp]
        public virtual void SetUp()
        {
            string assemblyName = "ZPFD." + TestContext.CurrentContext.Test.FullName.Replace("ProxyFoo.Tests.", "");
            var assemblyPath = assemblyName + ".dll";
            if (File.Exists(assemblyPath))
                File.Delete(assemblyPath);
            ProxyFooPolicies.ProxyModuleFactory = () => new ProxyModule(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ProxyFooPolicies.ClearProxyModule();
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (!ProxyModule.Default.IsAssemblyCreated)
                return;
            ProxyModule.Default.Save();
            if (TestContext.CurrentContext.Result.Status==TestStatus.Passed)
            {
                var assemblyPath = ProxyModule.Default.AssemblyName + ".dll";
                Assert.That(PeVerifyAssembly(".", assemblyPath), Is.EqualTo(0));
            }
            ProxyFooPolicies.ProxyModuleFactory = ProxyFooPolicies.DefaultProxyModuleFactory;
            ProxyFooPolicies.ClearProxyModule();
        }

        static int PeVerifyAssembly(string resultFolder, string assembly)
        {
            Console.WriteLine("*** Running PeVerify against {0} ***", assembly);
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = PeVerifyPath,
                    Arguments = assembly,
                    WorkingDirectory = resultFolder,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            p.Start();
            p.BeginOutputReadLine();
            if (!p.WaitForExit(5000))
            {
                p.Kill();
                Assert.Fail("The peverify process has timed out.");
            }
            Console.WriteLine(".");
            return p.ExitCode;
        }
    }
}