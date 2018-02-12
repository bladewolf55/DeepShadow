@echo off

msbuild.exe build.targets /t:package
pause
