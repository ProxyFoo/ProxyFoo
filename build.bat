msbuild "ProxyFoo.sln" /t:Rebuild /p:Configuration=Release
nuget pack build\ProxyFoo.nuspec -outputDirectory output -symbols