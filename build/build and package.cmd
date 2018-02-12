@echo off

msbuild.exe build.targets /t:build;package
pause
