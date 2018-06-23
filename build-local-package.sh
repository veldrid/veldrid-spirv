#!/usr/bin/env bash

scriptPath="`dirname \"$0\"`"

$scriptPath/build-native.sh Release
dotnet pack -c Release $scriptPath/src/Veldrid.SPIRV/Veldrid.SPIRV.csproj