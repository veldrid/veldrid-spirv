@setlocal
@echo off

call %~dp0ext/sync-shaderc.cmd
call %~dp0build-native.cmd Release