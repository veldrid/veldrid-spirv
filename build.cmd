@setlocal
@echo off

If NOT exist ".\build\" (
  mkdir build
)
pushd build
cmake -DCMAKE_GENERATOR_PLATFORM=x64 ..
popd
