@setlocal
@echo off

call .\build-native.cmd release win-x86
call .\build-native.cmd release win-x64
call dotnet pack -c Release src\Veldrid.SPIRV\Veldrid.SPIRV.csproj
