#!/usr/bin/env bash

scriptPath="`dirname \"$0\"`"

$scriptPath/ext/sync-shaderc.sh
$scriptPath/build-native.sh release

if [[ $(uname) == "Darwin" ]]; then
  $scriptPath/build-native.sh release ios
fi