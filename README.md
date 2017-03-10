#ProxyFoo
ProxyFoo is a library for the .NET Framework to facilitate creating high-performance proxies for Duck casting and other uses.

[Quick Start Guide](http://proxyfoo.com/docs/quickstart/)

Also see [proxyfoo.com](http://proxyfoo.com) for additional documentation.

##Notes
Requires Visual Studio 2017 or the .NET Core 1.0 SDK to build.

##.NET Core on Mac/Windows
To run the unit tests using .NET Core on the Mac or Windows (requires .NET Core 1.0 SDK from https://www.microsoft.com/net/core):

```
git clone https://github.com/ProxyFoo/ProxyFoo ProxyFoo
cd ProxyFoo
dotnet restore
dotnet run -f netcoreapp1.0 -p source/ProxyFooConsole/ProxyFooConsole.csproj tests
```