@echo off
rem add potential paths to PATH
path=%path%;C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin
path=%path%;C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin
path=%path%;C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin

rem Build Initializer
msbuild /t:Clean,Build /property:Configuration=Release ..\DeepShadow\DeepShadow.csproj 


rem Delete everything in Package
del Package\*.* /q

rem Package
nuget pack ..\DeepShadow\DeepShadow.csproj -o Package -Prop Configuration=Release

rem Publish
nuget push Package\*.nupkg -Source https://api.nuget.org/v3/index.json
