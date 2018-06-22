#!/usr/bin/env bash

scripthPath="`dirname \"$0\"`"

py $scripthPath/update_shaderc_sources.py --dir $scripthPath/shaderc --file $scripthPath/known_good.json
