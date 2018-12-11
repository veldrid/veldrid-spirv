@setlocal
@echo off

if not defined _ANDROID_NDK (
  set _ANDROID_NDK=C:\ProgramData\Microsoft\AndroidNDK64\android-ndk-r17
)

call %~dp0ext/sync-shaderc.cmd
call %~dp0build-native.cmd Release win-x64
call %~dp0build-native.cmd Release win-x86
call %~dp0build-native.cmd Release --android-ndk "%_ANDROID_NDK%" --android-abi arm64-v8a
call %~dp0build-native.cmd Release --android-ndk "%_ANDROID_NDK%" --android-abi armeabi-v7a
