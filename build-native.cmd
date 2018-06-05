@setlocal
@echo off

set BUILD_CONFIG=Debug

:ArgLoop
if [%1] == [] goto LocateVS
if /i [%1] == [Release]     ( set BUILD_CONFIG=Release&&shift&goto ArgLoop)
if /i [%1] == [Debug]       ( set BUILD_CONFIG=Debug&&shift&goto ArgLoop)
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

If NOT exist ".\build\" (
  mkdir build
)
pushd build
cmake -DCMAKE_GENERATOR_PLATFORM=x64 ..
popd

msbuild build\ALL_BUILD.vcxproj /p:Configuration=%BUILD_CONFIG%

:Success
exit /b 0

:NoVisualStudio
echo Unable to locate Visual Studio installation. Terminating.
exit /b 1