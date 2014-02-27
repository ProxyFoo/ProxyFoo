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

namespace ProxyFoo
{
    public static class Duck
    {
        /// <summary>
        /// Casts an object to the subject interface <typeparamref name="T"/> to the real subject <paramref name="o"/>, by
        /// creating a proxy to bind the methods of the subject to those of the real subject.
        /// </summary>
        /// <typeparam name="T">The subject interface</typeparam>
        /// <param name="o">The real subject</param>
        /// <returns>A proxy that implements T or null if the binding fails</returns>
        public static T Cast<T>(object o) where T : class
        {
            return DuckFactory.Default.MakeDuckProxyFor<T>(o);
        }

        /// <summary>
        /// Creates a function that will cast an object to the subject interface <typeparamref name="T"/> to the real subject
        /// passed into the function.  For repeated casts to the same subject interface, this function will outperform 
        /// repeated calls to <see cref="Duck.Cast{T}"/>.
        /// </summary>
        /// <typeparam name="T">The subject interface</typeparam>
        /// <returns>A function that is the equivalent of calling <see cref="Duck.Cast{T}"/> for the same subject</returns>
        public static Func<object, T> GetFastCaster<T>() where T : class
        {
            return DuckFactory.Default.GetFastCaster<T>();
        }
    }
}