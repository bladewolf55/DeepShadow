@echo off
rem add potential paths to PATH
path=%path%;C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin
path=%path%;C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin
path=%path%;C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin

rem Build Initializer
msbuild /property:Configuration=Release ..\DeepShadow\DeepShadow.csproj 

rem Package
nuget pack ..\DeepShadow\DeepShadow.nuspec -o Package
