#region Apache License Notice

// Copyright © 2013, Silverlake Software LLC
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProxyFoo;
using ProxyFoo.ExtensionApi;

namespace ProxyFooBenchmarks
{
    class Program
    {
        static bool _inPrime = true;
        static PerformanceCounter _pcGen0;
        static PerformanceCounter _pcGen1;
        static PerformanceCounter _pcGen2;
        static int _testSize;

        static void Main()
        {
            Console.WriteLine("Stopwatch is {0}high resolution.", Stopwatch.IsHighResolution ? "" : "NOT ");
            Console.WriteLine("Stopwatch is accurate to within {0}µs.", Math.Round((1.0 / Stopwatch.Frequency) * 1000000, 3));

            var processName = Process.GetCurrentProcess().ProcessName;
            _pcGen0 = new PerformanceCounter(".NET CLR Memory", "# Gen 0 Collections", processName);
            _pcGen1 = new PerformanceCounter(".NET CLR Memory", "# Gen 1 Collections", processName);
            _pcGen2 = new PerformanceCounter(".NET CLR Memory", "# Gen 2 Collections", processName);

            // Prime any JIT interference
            _testSize = 1;
            Console.WriteLine("---Benchmark Priming");
            Console.WriteLine("---First 'duck' benchmark represents bulk of priming for all 'duck' functions.");
            BenchmarksGroup1();
            BenchmarksGroup2();
            BenchmarksGroup3();
            BenchmarksGroup4();
            _inPrime = false;

            Console.WriteLine();
            Console.WriteLine("---Benchmark Tests");
            _testSize = 1000000;
            Console.WriteLine("---Test size is {0} units.", _testSize);
            BenchmarksGroup1();
            BenchmarksGroup2();
            BenchmarksGroup3();
            _testSize = 10000;
            Console.WriteLine("---Test size is {0} units.", _testSize);
            BenchmarksGroup4();
            _testSize = 1000;
            Console.WriteLine("---Test size is {0} units.", _testSize);
            BenchmarksGroup5();
        }

        public static void BenchmarksGroup1()
        {
            var testObjects = new SampleClass[_testSize];
            for (int i = 0; i < _testSize; ++i)
                testObjects[i] = new SampleClass();
            RunBenchmark("Direct", () => Direct(testObjects));
            long duckCastResult = RunBenchmark("DuckCast", () => DuckCast(testObjects));
            long fastDuckCastResult = RunBenchmark("FastDuckCast", () => FastDuckCast(testObjects));
            long dynamicResult = RunBenchmark("UsingDynamic", () => UsingDynamic(testObjects));
            var ducks = CreateDucks<IGetAnswer>(testObjects);
            var badDucks = CreateDucks<IDontGetAnswer>(testObjects);
            RunBenchmark("DuckCall", () => DuckCall(ducks));
            RunBenchmark("DuckMissingMethodMethodExistsCall", () => DuckMissingMethodMethodExistsCall(badDucks));

            // This result is based on the fact that making two dynamic calls doubles the overhead of using
            // dynamic, while two duck calls has almost no additional overhead.
            if (_inPrime)
                return;
            Console.WriteLine("* DuckCast beats dynamic after {0} method calls.", Math.Round((double)duckCastResult / dynamicResult, 2));
            Console.WriteLine("* FastDuckCast beats dynamic after {0} method calls.", Math.Round((double)fastDuckCastResult / dynamicResult, 2));
        }

        public static void BenchmarksGroup2()
        {
            var testObjects = new SampleClass2[_testSize];
            for (int i = 0; i < _testSize; ++i)
                testObjects[i] = new SampleClass2();
            // ReSharper disable CoVariantArrayConversion
            RunBenchmark("DuckObjectThatSupportsDirect", () => DuckObjectThatSupportsDirect(testObjects));
            RunBenchmark("DirectCastObjectThatSupportsDirect", () => DirectCastObjectThatSupportsDirect(testObjects));
            RunBenchmark("DynamicCallObjectThatSupportsDirect", () => DynamicCallObjectThatSupportsDirect(testObjects));
            RunBenchmark("SafeCallOnNonNullObject", () => SafeCallOnNonNullObject(testObjects));
            // ReSharper restore CoVariantArrayConversion
        }

        public static void BenchmarksGroup3()
        {
            var testObjects = new SampleClass2[_testSize];
            // ReSharper disable CoVariantArrayConversion
            RunBenchmark("SafeCallOnNullObject", () => SafeCallOnNullObject(testObjects));
            // ReSharper restore CoVariantArrayConversion
        }

        public static void BenchmarksGroup4()
        {
            var testObjects = new SampleClass[_testSize];
            for (int i = 0; i < _testSize; ++i)
                testObjects[i] = new SampleClass();
            // ReSharper disable CoVariantArrayConversion
            RunBenchmark("LowLevelCachedDuckProxyTypeCreation", () => LowLevelCachedDuckProxyTypeCreation(testObjects));
            var badDucks = CreateDucks<IDontGetAnswer>(testObjects);
            RunBenchmark("DuckMissingMethodExceptionCall", () => DuckMissingMethodExceptionCall(badDucks));
            RunBenchmark("DynamicMissingMethodExceptionCall", () => DynamicMissingMethodExceptionCall(badDucks));
            // ReSharper restore CoVariantArrayConversion
        }

        public static void BenchmarksGroup5()
        {
            var testObjects = new SampleClass[_testSize];
            for (int i = 0; i < _testSize; ++i)
                testObjects[i] = new SampleClass();
            // ReSharper disable CoVariantArrayConversion
            RunBenchmark("RawDuckProxyTypeCreation", () => RawDuckProxyTypeCreation(testObjects));
            // ReSharper restore CoVariantArrayConversion
        }

        static void Reset()
        {
            ProxyFooPolicies.ClearProxyModule();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static long RunBenchmark(string name, Action benchmark)
        {
            Reset();

            var sw = Stopwatch.StartNew();
            var gcGen0Total = _pcGen0.RawValue;
            var gcGen1Total = _pcGen1.RawValue;
            var gcGen2Total = _pcGen2.RawValue;
            benchmark();
            var gcGen0Count = _pcGen0.RawValue - gcGen0Total;
            var gcGen1Count = _pcGen1.RawValue - gcGen1Total;
            var gcGen2Count = _pcGen2.RawValue - gcGen2Total;
            sw.Stop();
            if (_inPrime)
            {
                if (sw.ElapsedMilliseconds > 0)
                    Console.WriteLine("{0} Priming = {1}ms.", name, sw.ElapsedMilliseconds);
            }
            else
            {
                var perUnit = Math.Round((double)sw.ElapsedMilliseconds / _testSize * 1000.0, 3);
                Console.WriteLine("{0} = {1}ms. ({2}µs per unit, {3}/{4}/{5} GCs)", name, sw.ElapsedMilliseconds, perUnit,
                    gcGen0Count, gcGen1Count, gcGen2Count);
            }

            return sw.ElapsedMilliseconds;
        }

        public static int Direct(SampleClass[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                sum += testObjects[i].GetAnswer();
            }
            return sum;
        }

        public static int DuckCast(SampleClass[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                var result = Duck.Cast<IGetAnswer>(testObjects[i]);
                sum += result.GetAnswer();
            }
            return sum;
        }

        public static int FastDuckCast(SampleClass[] testObjects)
        {
            var caster = DuckFactory.Default.GetFastCaster<IGetAnswer>();
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                var result = caster(testObjects[i]);
                sum += result.GetAnswer();
            }
            return sum;
        }

        public static int UsingDynamic(SampleClass[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                dynamic result = testObjects[i];
                sum += result.GetAnswer();
            }
            return sum;
        }

        public static int DuckObjectThatSupportsDirect(object[] testObjects)
        {
            var caster = DuckFactory.Default.GetFastCaster<IGetAnswer>();
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                var result = caster(testObjects[i]);
                sum += result.GetAnswer();
            }
            return sum;
        }

        public static int DirectCastObjectThatSupportsDirect(object[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                var result = testObjects[i] as IGetAnswer;
                if (result==null)
                    continue;
                sum += result.GetAnswer();
            }
            return sum;
        }

        public static int DynamicCallObjectThatSupportsDirect(object[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                dynamic result = testObjects[i];
                sum += result.GetAnswer();
            }
            return sum;
        }

        public static int SafeCallOnNonNullObject(IGetAnswer[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                sum += Safe.Call(testObjects[i], a => a.GetAnswer());
            }
            return sum;
        }

        /// <summary>
        /// The intent of this benchmark is to measure the performance of the low-level cache in ProxyModule.
        /// </summary>
        public static int LowLevelCachedDuckProxyTypeCreation(object[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                var duck = (IGetAnswer)DuckFactory.Default.MakeDuckProxyForInterfaces(testObjects[i], typeof(IGetAnswer));
                sum += duck.GetAnswer();
            }
            return sum;
        }

        /// <summary>
        /// The intent of this benchmark is to measure the raw creation and build time of a proxy type.
        /// </summary>
        public static int RawDuckProxyTypeCreation(object[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                var duck = (IGetAnswer)DuckFactory.Default.MakeDuckProxyForInterfaces(testObjects[i], typeof(IGetAnswer));
                sum += duck.GetAnswer();
                ProxyFooPolicies.ClearProxyModule();
            }
            return sum;
        }

        static T[] CreateDucks<T>(IEnumerable<object> testObjects) where T : class
        {
            return testObjects.Select(a => a.Duck<T>()).ToArray();
        }

        public static int DuckCall(IGetAnswer[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                sum += testObjects[i].GetAnswer();
            }
            return sum;
        }

        public static int DuckMissingMethodExceptionCall(IDontGetAnswer[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                // ReSharper disable once EmptyGeneralCatchClause
                try
                {
                    sum += testObjects[i].DoesNotExist();
                }
                catch {}
            }
            return sum;
        }

        public static int DynamicMissingMethodExceptionCall(IDontGetAnswer[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                // ReSharper disable once EmptyGeneralCatchClause
                try
                {
                    dynamic testObject = testObjects[i];
                    sum += testObject.DoesNotExist();
                }
                catch {}
            }
            return sum;
        }

        public static int DuckMissingMethodMethodExistsCall(IDontGetAnswer[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                if (testObjects[i].MethodExists<IDontGetAnswer>(a => a.DoesNotExist()))
                    sum += testObjects[i].DoesNotExist();
            }
            return sum;
        }

        public static int SafeCallOnNullObject(IGetAnswer[] testObjects)
        {
            int sum = 0;
            for (int i = 0; i < _testSize; ++i)
            {
                sum += Safe.Call(testObjects[i], a => a.GetAnswer());
            }
            return sum;
        }
    }
}