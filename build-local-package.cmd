@setlocal
@echo off

call .\build-native.cmd Release win-x86 --artifact-name build\Release\win-x86\libveldrid-spirv.dll
call .\build-native.cmd Release win-x64 --artifact-name build\Release\win-x64\libveldrid-spirv.dll
call dotnet pack -c Release src\Veldrid.SPIRV\Veldrid.SPIRV.csproj
