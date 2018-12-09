#!/usr/bin/env bash

scriptPath="`dirname \"$0\"`"

which python
python --version
exit

$scriptPath/ext/sync-shaderc.sh
$scriptPath/build-native.sh release

if [[ $(uname) == "Darwin" ]]; then
  $scriptPath/build-native.sh release ios
fi