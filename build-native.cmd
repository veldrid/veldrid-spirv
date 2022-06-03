@setlocal
@echo off

set _CMAKE_BUILD_TYPE=Debug
set _BUILD_ARCH=x64
set _CMAKE_GENERATOR_PLATFORM=x64
set _NDK_DIR=
set _ANDROID_ABI=arm64-v8a
set _OS_DIR=
set _ANDROID_PLATFORM=android-16

:ArgLoop
if [%1] == [] goto LocateVS
if /i [%1] == [release] (set _CMAKE_BUILD_TYPE=Release&& shift & goto ArgLoop)
if /i [%1] == [debug] (set _CMAKE_BUILD_TYPE=Debug&& shift & goto ArgLoop)
if /i [%1] == [win-x64] (set _BUILD_ARCH=x64&& set _CMAKE_GENERATOR_PLATFORM=x64&& shift & goto ArgLoop)
if /i [%1] == [win-x86] (set _BUILD_ARCH=x86&& set _CMAKE_GENERATOR_PLATFORM=Win32&& shift & goto ArgLoop)
if /i [%1] == [android] (set _ANDROID_ABI=%2&& set _BUILD_ARCH=%2&& shift && shift & goto ArgLoop)
if /i [%1] == [--android-ndk] (set _NDK_DIR=%2&& shift && shift & goto ArgLoop)
if /i [%1] == [--android-platform] (set _ANDROID_PLATFORM=%2&& shift && shift & goto ArgLoop)
if /i [%1] == [--python-exe] (set _PYTHON_EXECUTABLE=%2&& shift && shift & goto ArgLoop)
shift
goto ArgLoop

:LocateVS

set _CMAKE_ARGS=-DCMAKE_BUILD_TYPE=%_CMAKE_BUILD_TYPE%
if defined _NDK_DIR (
  set _CMAKE_ARGS=%_CMAKE_ARGS% -G "MinGW Makefiles" -DANDROID_ABI=%_ANDROID_ABI% -DANDROID_PLATFORM=%_ANDROID_PLATFORM% -DCMAKE_MAKE_PROGRAM="%_NDK_DIR%\prebuilt\windows-x86_64\bin\make.exe" -DCMAKE_TOOLCHAIN_FILE="%_NDK_DIR%\build\cmake\android.toolchain.cmake" -DCMAKE_CXX_FLAGS_RELEASE=-g0
  set _OS_DIR=android
  set _BUILD_ARCH=%_ANDROID_ABI%
) else (
  set _CMAKE_ARGS=%_CMAKE_ARGS% -DCMAKE_GENERATOR_PLATFORM=%_CMAKE_GENERATOR_PLATFORM%
  set _OS_DIR=win
)

set _BUILD_DIR=.\build\%_CMAKE_BUILD_TYPE%\%_OS_DIR%-%_BUILD_ARCH%

if defined _PYTHON_EXECUTABLE (
  set _CMAKE_ARGS=%_CMAKE_ARGS% -DPYTHON_EXECUTABLE=%_PYTHON_EXECUTABLE%

)

set _CMAKE_ARGS=%_CMAKE_ARGS% ..\..\..

If NOT exist "%BUILD_DIR%" (
  mkdir %_BUILD_DIR%
)
pushd %_BUILD_DIR%
cmake %_CMAKE_ARGS%
cmake --build . --config %_CMAKE_BUILD_TYPE% --target veldrid-spirv

if "%_OS_DIR%" == "win" (
copy .\%_CMAKE_BUILD_TYPE%\* .\
)

popd

:Success
exit /b 0

:NoVisualStudio
echo Unable to locate Visual Studio installation. Terminating.
exit /b 1
