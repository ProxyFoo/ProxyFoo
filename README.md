#ProxyFoo
ProxyFoo is a library for the .NET Framework to facilitate creating high-performance proxies for Duck casting and other uses.

Version 1 will most likely remain focused on supporting Duck casting and Safe wrapping.  In later versions I hope to expand into areas
that may have some overlap (but using a different approach) with Castle DynamicProxy.

##Duck Casting

A duck cast can be created using the following static method call:

```c#
var result = Duck.Cast<ISample>(obj);
```

There will be a small amount of overhead in the first call, but 
significant effort was made to maximum the performance of multiple casts.  
Calling a method repeatedly or multiple methods on the resulting proxy object 
has almost no overhead.  A cast of this form performs better than using dynamic 
after about 3 method calls.

For repeated casts to the same subject interface a higher performance option
is available in the following form:

```c#
var fastCaster = Duck.GetFastCaster<ISample>();
var result = fastCaster(obj);
```

This is useful for a factory scenario or when casting multiple objects in a
loop.  This reduces overhead such that a cast of this form performs better than
using dynamic after about 2 method calls.

**Not Yet Supported**: 
Generics, Recursively defined types, out parameters