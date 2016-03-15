#ProxyFoo
ProxyFoo is a library for the .NET Framework to facilitate creating high-performance proxies for Duck casting and other uses.

[Quick Start Guide](http://proxyfoo.com/docs/quickstart/)

Also see [proxyfoo.com](http://proxyfoo.com) for additional documentation.

##Notes
Requires Visual Studio 2015 to use new project files.  This library now uses ASP.NET Core package projects to build the nuget package.
The .NET 4 project has been left to make it easier to work with the Resharper test runner
until tests can be run directly from the xproj or project.json.  All the work here is
based on ASP.NET 5 RC Update 1.

The CoreCLR version does run on the Mac with all tests passing but that's all I've tested so far.
There's a good chance I've done something incorrectly so if you see something please
let me know.

Here are some of the very helpful posts I read to work this all out:
- http://blog.marcgravell.com/2015/11/the-road-to-dnx-part-1.html
- https://oren.codes/2016/02/08/project-json-all-the-things/

##DNX on Mac/Windows
To run the unit tests using DNX on the Mac or Windows (requires ASP.NET 5 RC Update 1 from https://get.asp.net/):

```
git clone https://github.com/ProxyFoo/ProxyFoo ProxyFoo
cd ProxyFoo
dnu restore
dnx -p source/ProxyFooConsole tests
```



