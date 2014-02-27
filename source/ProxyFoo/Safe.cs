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
using ProxyFoo.Core.SubjectTypes;

namespace ProxyFoo
{
    public static class Safe
    {
        public static TResult Call<T, TResult>(T target, Func<T, TResult> safeCall)
        {
            return safeCall(Wrap<T>(target));
        }

        public static TResult CallAndUnwrap<T, TResult>(T target, Func<T, TResult> safeCall) where TResult : class
        {
            return Unwrap(safeCall(Wrap<T>(target)));
        }

        public static T Wrap<T>(object o)
        {
            return SafeFactory.Default.MakeSafeProxyFor<T>(o);
        }

        public static T Unwrap<T>(T o) where T : class
        {
            if (o==null)
                return null;

            var meta = o as ISafeProxyMeta;
            return meta==null ? o : (T)meta.Unwrap();
        }

        public static bool IsNull(object o)
        {
            return Unwrap(o)==null;
        }
    }
}