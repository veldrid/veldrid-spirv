#!/usr/bin/env bash

scriptPath="`dirname \"$0\"`"

_CMakeBuildType=Debug

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
        *)
            __UnprocessedBuildArgs="$__UnprocessedBuildArgs $1"
    esac

    shift
done

mkdir -p $scriptPath/build/$_CMakeBuildType
pushd $scriptPath/build/$_CMakeBuildType
cmake ../.. -DCMAKE_BUILD_TYPE=$_CMakeBuildType
make
popd
