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
using ProxyFoo.Core;
using ProxyFoo.Core.SubjectTypes;

namespace ProxyFoo
{
    public static class MethodExists
    {
        /// <summary>
        /// Returns whether or not the method exists on a given object.  This is a very low overhead call
        /// for proxies that support <see cref="IMethodExistsProxyMeta"/>
        /// </summary>
        /// <typeparam name="T">The subject interface the contains the method</typeparam>
        /// <param name="o">The object to test for inclusion of the method</param>
        /// <param name="action">An examplar of the call to the method or property.  Arguments will be evaluated (but not used)
        /// so be careful to use dummy arguments to avoid side effects.</param>
        /// <returns>true if the method exists, false if it does not</returns>
        public static bool On<T>(object o, Action<T> action) where T : class
        {
            if (o==null)
                return false;

            ISubjectMethodExists<T> subjectMethodExists = null;

            // Check if this is a proxy that supports MethodExists directly
            var meta = o as IMethodExistsProxyMeta;
            if (meta!=null)
                subjectMethodExists = meta.GetSubjectMethodExists<T>();

            // It's a regular object so if it implements T, then it's an easy yes.
            // Note that a proxy that always implements all methods can choose to not implement IMethodExistsProxyMeta or
            // a specific subject could return null to fall through to this case.
            if (subjectMethodExists==null && (o as T)!=null)
                return true;

            // It's a regular object that does not implement T, so lets see if a duck cast for T would work.
            subjectMethodExists = subjectMethodExists ?? DuckFactory.Default.MakeSubjectMethodExistsForDuckProxy<T>(o);
            int methodIndex = MethodIndexFactory.Default.GetMethodIndex(action);
            return subjectMethodExists.DoesMethodExist(methodIndex);
        }

        /// <summary>
        /// Returns whether or not the method exists on a given object.  This is a very low overhead call
        /// for proxies that support <see cref="IMethodExistsProxyMeta"/>.  Note that this overload taking a func
        /// was required because the compiler would not allow only a property access as an action.
        /// </summary>
        /// <typeparam name="T">The subject interface the contains the method</typeparam>
        /// <typeparam name="TOut">The return type of the method or property</typeparam>
        /// <param name="o">The object to test for inclusion of the method</param>
        /// <param name="func">An examplar of the call to the method or property.  Arguments will be evaluated (but not used)
        /// so be careful to use dummy arguments to avoid side effects.</param>
        /// <returns>true if the method exists, false if it does not</returns>    
        /// <remarks>This overload does not 'wrap' the func as an action because this would cause an allocation
        /// on every call.  It also avoids using Func&lt;T, object&gt; which also caused allocations.</remarks>
        public static bool On<T, TOut>(object o, Func<T, TOut> func) where T : class
        {
            if (o==null)
                return false;

            ISubjectMethodExists<T> subjectMethodExists = null;

            // Check if this is a proxy that supports MethodExists directly
            var meta = o as IMethodExistsProxyMeta;
            if (meta!=null)
                subjectMethodExists = meta.GetSubjectMethodExists<T>();

            // It's a regular object so if it implements T, then it's an easy yes.
            // Note that a proxy that always implements all methods can choose to not implement IMethodExistsProxyMeta or
            // a specific subject could return null to fall through to this case.
            if (subjectMethodExists==null && (o as T)!=null)
                return true;

            // It's a regular object that does not implement T, so lets see if a duck cast for T would work.
            subjectMethodExists = subjectMethodExists ?? DuckFactory.Default.MakeSubjectMethodExistsForDuckProxy<T>(o);
            int methodIndex = MethodIndexFactory.Default.GetMethodIndex(func);
            return subjectMethodExists.DoesMethodExist(methodIndex);
        }
    }
}