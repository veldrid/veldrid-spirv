#!/usr/bin/env bash

scripthPath="`dirname \"$0\"`"

$scripthPath/build-native.sh Release
dotnet pack -c Release $scripthPath/src/Veldrid.SPIRV/Veldrid.SPIRV.csproj