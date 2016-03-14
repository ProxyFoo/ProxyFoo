msbuild "ProxyFoo.sln" /t:Rebuild /p:Configuration=Release
rem nuget pack build\ProxyFoo.nuspec -outputDirectory output -symbols