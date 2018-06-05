@setlocal
@echo off

call .\build-native.cmd Release
call dotnet pack -c Release src\Veldrid.SPIRV\Veldrid.SPIRV.csproj
