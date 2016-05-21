#ProxyFoo
ProxyFoo is a library for the .NET Framework to facilitate creating high-performance proxies for Duck casting and other uses.

[Quick Start Guide](http://proxyfoo.com/docs/quickstart/)

Also see [proxyfoo.com](http://proxyfoo.com) for additional documentation.

##Notes
Requires Visual Studio 2015 to use new project files.  This library now uses .NET Core RC2 projects to build the nuget package.
The .NET 4 project has been left to make it easier to work with the Resharper test runner
until tests can be run directly from the xproj or project.json.

The CoreCLR version does run on the Mac with all tests passing but that's all I've tested so far.
There's a good chance I've done something incorrectly so if you see something please
let me know.

Here are some of the very helpful posts I read to work this all out:
- http://blog.marcgravell.com/2015/11/the-road-to-dnx-part-1.html
- https://oren.codes/2016/02/08/project-json-all-the-things/

##.NET Core on Mac/Windows
To run the unit tests using .NET Core on the Mac or Windows (requires .NET Core RC2 from https://www.microsoft.com/net/core):

```
git clone https://github.com/ProxyFoo/ProxyFoo ProxyFoo
cd ProxyFoo
dotnet restore
dotnet run -p source/ProxyFooConsole tests
```

NOTE: If you use the default build folders from dotnet, the "-Net40" projects will not build until you
clean the output of the project folders (bin and obj).



