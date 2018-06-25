# Veldrid.SPIRV

Veldrid.SPIRV is an extension library for [Veldrid](https://github.com/mellinoe/veldrid) that provides support for loading SPIR-V bytecode for all Veldrid backends.

## libveldrid-spirv

Veldrid.SPIRV is implemented primarily as a native library, interfacing with [SPIRV-Cross](https://github.com/KhronosGroup/SPIRV-Cross) and [shaderc](https://github.com/google/shaderc). There are build scripts in the root of the repository which can be used to automatically build the native library for your platform.

Build requirements:

* CMake
* Python

Veldrid.SPIRV is available on NuGet:

[![NuGet](https://img.shields.io/nuget/v/Veldrid.SPIRV.svg)](https://www.nuget.org/packages/Veldrid.SPIRV)
