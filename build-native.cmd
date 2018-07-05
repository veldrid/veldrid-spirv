@setlocal
@echo off

set BUILD_CONFIG=Debug
set BUILD_ARCH=x64
set BUILD_CMAKE_GENERATOR_PLATFORM=x64

:ArgLoop
if [%1] == [] goto LocateVS
if /i [%1] == [Release] (set BUILD_CONFIG=Release&& shift & goto ArgLoop)
if /i [%1] == [Debug] (set BUILD_CONFIG=Debug&& shift & goto ArgLoop)
if /i [%1] == [x64] (set BUILD_ARCH=x64&& shift & goto ArgLoop)
if /i [%1] == [x86] (set BUILD_ARCH=x86&& set BUILD_CMAKE_GENERATOR_PLATFORM=Win32&&shift & goto ArgLoop)
shift
goto ArgLoop

:LocateVS
set _VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %_VSWHERE% (
  for /f "usebackq tokens=*" %%i in (`%_VSWHERE% -latest -property installationPath`) do set _VSCOMNTOOLS=%%i\Common7\Tools
)
if not exist "%_VSCOMNTOOLS%" set _VSCOMNTOOLS=%VS140COMNTOOLS%
if not exist "%_VSCOMNTOOLS%" goto :NoVisualStudio

call "%_VSCOMNTOOLS%\VsDevCmd.bat"

If NOT exist ".\build\%BUILD_ARCH%" (
  mkdir build\%BUILD_ARCH%
)
pushd build\%BUILD_ARCH%
cmake -DCMAKE_GENERATOR_PLATFORM=%BUILD_CMAKE_GENERATOR_PLATFORM% ..\..
popd

echo Calling msbuild build\%BUILD_ARCH%\ALL_BUILD.vcxproj /p:Configuration=%BUILD_CONFIG%

msbuild build\%BUILD_ARCH%\ALL_BUILD.vcxproj /p:Configuration=%BUILD_CONFIG%

:Success
exit /b 0

:NoVisualStudio
echo Unable to locate Visual Studio installation. Terminating.
exit /b 1
