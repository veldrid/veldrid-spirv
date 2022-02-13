#!/usr/bin/env bash

scriptPath="$( cd "$(dirname "$0")" ; pwd -P )"
_CMakeBuildType=Debug
_CMakeToolchain=
_CMakeIOSPlatform=
_CMakeEnableBitcode=
_OutputPathPrefix=
_CMakeBuildTarget=veldrid-spirv
_CMakeOsxArchitectures=
_CMakeGenerator=
_CMakeExtraBuildArgs=

while :; do
    if [ $# -le 0 ]; then
        break
    fi

    lowerI="$(echo $1 | awk '{print tolower($0)}')"
    case $lowerI in
        debug|-debug)
            _CMakeBuildType=Debug
            ;;
        release|-release)
            _CMakeBuildType=Release
            ;;
        ios)
            _CMakeToolchain=-DCMAKE_TOOLCHAIN_FILE=$scriptPath/ios/ios.toolchain.cmake
            _CMakePlatform=-DPLATFORM=OS64COMBINED
            _CMakeEnableBitcode=-DENABLE_BITCODE=0
            _OutputPathPrefix=ios-
            _CMakeBuildTarget=veldrid-spirv-combined_genfile
            _CMakeGenerator="-G Xcode -T buildsystem=1"
            _CMakeExtraBuildArgs="--config Release"
            ;;
        -osx-architectures)
            _CMakeOsxArchitectures=$2
            shift
            ;;
        *)
            __UnprocessedBuildArgs="$__UnprocessedBuildArgs $1"
    esac

    shift
done

_OutputPath=$scriptPath/build/$_OutputPathPrefix$_CMakeBuildType
_PythonExePath=$(which python3)
if [[ $_PythonExePath == "" ]]; then
    echo Build failed: could not locate python executable.
    exit 1
fi

mkdir -p $_OutputPath
pushd $_OutputPath
cmake ../.. -DCMAKE_BUILD_TYPE=$_CMakeBuildType $_CMakeGenerator $_CMakeToolchain $_CMakePlatform $_CMakeEnableBitcode -DPYTHON_EXECUTABLE=$_PythonExePath -DCMAKE_OSX_ARCHITECTURES="$_CMakeOsxArchitectures"
cmake --build . --target $_CMakeBuildTarget $_CMakeExtraBuildArgs
popd
