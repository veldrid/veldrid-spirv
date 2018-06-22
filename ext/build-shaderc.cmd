@setlocal
@echo off

py %~dp0update_shaderc_sources.py --dir %~dp0shaderc --file %~dp0known_good.json
mkdir %~dp0shaderc\build
pushd %~dp0shaderc\build
cmake -DSHADERC_SKIP_INSTALL=ON -DSHADERC_SKIP_TESTS=ON -DSHADERC_ENABLE_SHARED_CRT=ON -DCMAKE_GENERATOR_PLATFORM=x64 ..
cmake --build . --config Debug
popd
