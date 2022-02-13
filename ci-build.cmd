@setlocal
@echo off

if not defined ANDROID_NDK_HOME (
  set ANDROID_NDK_HOME=C:\ProgramData\Microsoft\AndroidNDK64\android-ndk-r17
)

call %~dp0ext/sync-shaderc.cmd
call %~dp0build-native.cmd Release win-x64 --artifact-name build\win-x64\libveldrid-spirv.dll
call %~dp0build-native.cmd Release win-x86 --artifact-name build\win-x86\libveldrid-spirv.dll
call %~dp0build-native.cmd Release --android-ndk "%ANDROID_NDK_HOME%" --android-abi arm64-v8a --artifact-name build\android-arm64-v8a\libveldrid-spirv.so
call %~dp0build-native.cmd Release --android-ndk "%ANDROID_NDK_HOME%" --android-abi x86_64 --artifact-name build\android-x86_64\libveldrid-spirv.so
call %~dp0build-native.cmd Release --android-ndk "%ANDROID_NDK_HOME%" --android-abi armeabi-v7a --artifact-name build\android-armeabi-v7a\libveldrid-spirv.so
