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

namespace ProxyFoo.ExtensionApi
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Casts an object to the subject interface <typeparamref name="T"/> from the real subject <paramref name="o"/>, by
        /// creating a proxy that duck binds to the methods of the subject to those of the real subject.
        /// </summary>
        /// <typeparam name="T">The subject interface</typeparam>
        /// <param name="o">The real subject</param>
        /// <returns>A proxy that implements T or null if the binding fails</returns>
        public static T Duck<T>(this object o) where T : class
        {
            return ProxyFoo.Duck.Cast<T>(o);
        }

        /// <summary>
        /// Returns whether or not the method exists on a given object.  This is a very low overhead call
        /// for objects that are duck proxies.
        /// </summary>
        /// <typeparam name="T">The subject interface the contains the method</typeparam>
        /// <param name="o">The object to test for inclusion of the method</param>
        /// <param name="action">An examplar of the call to the method or property.  Arguments will be evaluated (but not used)
        /// so be careful to use dummy arguemnts to avoid side effects.</param>
        /// <returns>True if the method exists, false if it does not</returns>
        public static bool MethodExists<T>(this object o, Action<T> action) where T : class
        {
            return ProxyFoo.MethodExists.On(o, action);
        }

        /// <summary>
        /// Returns whether or not the method exists on a given object.  This is a very low overhead call
        /// for objects that are duck proxies.  Note that this overload taking a func
        /// was required because the compiler would not allow only a property access as an action.
        /// </summary>
        /// <typeparam name="T">The subject interface the contains the method</typeparam>
        /// <typeparam name="TOut">The return type of the method or property</typeparam>
        /// <param name="o">The object to test for inclusion of the method or property</param>
        /// <param name="func">An examplar of the call to the method or property.  Arguments will be evaluated (but not used)
        /// so be careful to use dummy arguemnts to avoid side effects.</param>
        /// <returns>True if the method exists, false if it does not</returns>
        public static bool MethodExists<T, TOut>(this object o, Func<T, TOut> func) where T : class
        {
            return ProxyFoo.MethodExists.On(o, func);
        }

        /// <summary>
        /// Casts an object to the subject interface <typeparamref name="T"/> from the real subject <paramref name="o"/>, by
        /// creating a proxy to call methods of the subject without null exceptions.
        /// </summary>
        /// <typeparam name="T">The subject interface</typeparam>
        /// <param name="o">The real subject</param>
        /// <returns>A proxy that implements T or null if the binding fails</returns>
        public static T Safe<T>(this object o) where T : class
        {
            return ProxyFoo.Safe.Wrap<T>(o);
        }

        /// <summary>
        /// Checks if this object represents a null (i.e. is a safe proxy to null).
        /// </summary>
        /// <param name="o">The object to test</param>
        /// <returns>True if the object is null or a safe proxy to null, false otherwise</returns>
        public static bool IsNull(this object o)
        {
            return ProxyFoo.Safe.IsNull(o);
        }

        /// <summary>
        /// Unwraps a safe proxy
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="o">The safe proxy to unwrap</param>
        /// <returns>The real subject of the proxy which can be null for a safe proxy to null.  If the object
        /// is not a proxy then the object is returned as is.</returns>
        public static T Unwrap<T>(this T o) where T : class
        {
            return ProxyFoo.Safe.Unwrap(o);
        }
    }
}