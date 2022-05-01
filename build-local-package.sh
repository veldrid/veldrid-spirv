#!/usr/bin/env bash

scriptPath="`dirname \"$0\"`"

_BuildConfig=Debug
_Platform=
_Arguments=

while :; do
    if [ $# -le 0 ]; then
        break
    fi

    lowerI="$(echo $1 | awk '{print tolower($0)}')"
    case $lowerI in
        debug|-debug)
            _BuildConfig=Debug
            ;;
        release|-release)
            _BuildConfig=Release
            ;;
        osx)
            _Platform=osx
            _Arguments=$2
            shift
            ;;
        linux-x64)
            _Platform=linux-x64
            ;;
        *)
            __UnprocessedBuildArgs="$__UnprocessedBuildArgs $1"
    esac

    shift
done

$scriptPath/build-native.sh $_BuildConfig $_Platform $_Arguments
dotnet pack -c $_BuildConfig $scriptPath/src/Veldrid.SPIRV/Veldrid.SPIRV.csproj
