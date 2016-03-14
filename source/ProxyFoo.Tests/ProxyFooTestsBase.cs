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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
#if FEATURE_PEVERIFY
using Microsoft.Build.Utilities;
#endif
using NUnit.Framework;
#if NUNIT3
using NUnit.Framework.Interfaces;
#endif

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
#if FEATURE_PEVERIFY
        static readonly string PeVerifyPath;

        static ProxyFooTestsBase()
        {
            // The +1 was necessary to work on a machine with only .NET 4.5.
            PeVerifyPath = /*ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("peverify.exe", TargetDotNetFrameworkVersion.Version40)
                           ??*/ ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("peverify.exe", TargetDotNetFrameworkVersion.Version40 + 1);
            Assert.That(File.Exists(PeVerifyPath));
        }
#endif
        [SetUp]
        public virtual void SetUp()
        {
            string assemblyName = "ZPFD." + TestContext.CurrentContext.Test.FullName.Replace("ProxyFoo.Tests.", "");
            foreach (var c in Path.GetInvalidFileNameChars())
                assemblyName = assemblyName.Replace(c, '_');
            var assemblyPath = assemblyName + ".dll";
#if FEATURE_PEVERIFY
            if (File.Exists(assemblyPath))
                File.Delete(assemblyPath);
            ProxyFooPolicies.ProxyModuleFactory = () => new ProxyModule(assemblyName, AssemblyBuilderAccess.RunAndSave);
#else
            ProxyFooPolicies.ProxyModuleFactory = () => new ProxyModule(assemblyName, AssemblyBuilderAccess.Run);
#endif
            ProxyFooPolicies.ClearProxyModule();
        }

        [TearDown]
        public virtual void TearDown()
        {
#if FEATURE_PEVERIFY
            if (!ProxyModule.Default.IsAssemblyCreated)
                return;
            ProxyModule.Default.Save();
            var assemblyPath = ProxyModule.Default.AssemblyName + ".dll";
#if NUNIT3
            if (TestContext.CurrentContext.Result.Outcome.Status==TestStatus.Passed)
#else
            if (TestContext.CurrentContext.Result.Status==TestStatus.Passed)
#endif
            {
                Assert.That(PeVerifyAssembly(".", assemblyPath), Is.EqualTo(0));
            }
#endif
            ProxyFooPolicies.ProxyModuleFactory = ProxyFooPolicies.DefaultProxyModuleFactory;
            ProxyFooPolicies.ClearProxyModule();
        }

#if FEATURE_PEVERIFY
        static int PeVerifyAssembly(string resultFolder, string assembly)
        {
            var output = new StringBuilder();
            output.AppendLine("*** Running PeVerify against " + assembly + " ***");
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = PeVerifyPath,
                    Arguments = assembly + " /nologo",
                    WorkingDirectory = resultFolder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.OutputDataReceived += (s, e) => output.AppendLine(e.Data);
            p.Start();
            p.BeginOutputReadLine();
            if (!p.WaitForExit(5000))
            {
                p.Kill();
                Console.Write(output);
                Console.WriteLine(".");
                Assert.Fail("The peverify process has timed out.");
            }
            if (p.ExitCode!=0)
            {
                Console.Write(output);
                Console.WriteLine(".");
            }
            return p.ExitCode;
        }
#endif
    }
}