@setlocal
@echo off

call .\build-native.cmd Release x86
call .\build-native.cmd Release x64
call dotnet pack -c Release src\Veldrid.SPIRV\Veldrid.SPIRV.csproj
