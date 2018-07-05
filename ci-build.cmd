@setlocal
@echo off

call %~dp0ext/sync-shaderc.cmd
call %~dp0build-native.cmd Release x64
copy %~dp0build\x64\Release\libveldrid-spirv.dll %~dp0build\x64\Release\libveldrid-spirv.win-x64.dll
call %~dp0build-native.cmd Release x86
copy %~dp0build\x86\Release\libveldrid-spirv.dll %~dp0build\x86\Release\libveldrid-spirv.win-x86.dll
