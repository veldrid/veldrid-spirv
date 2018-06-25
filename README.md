# Veldrid.SPIRV

[![NuGet](https://img.shields.io/nuget/v/Veldrid.SPIRV.svg)](https://www.nuget.org/packages/Veldrid.SPIRV)

Veldrid.SPIRV is an extension library for [Veldrid](https://github.com/mellinoe/veldrid) that provides support for loading SPIR-V bytecode for all Veldrid backends.

## Usage

The easiest way to use Veldrid.SPIRV is through [the extension methods it provides for the ResourceFactory type](https://github.com/mellinoe/veldrid-spirv/blob/master/src/Veldrid.SPIRV/ResourceFactoryExtensions.cs).

```C#
byte[] vertexShaderSpirvBytes = File.ReadAllBytes("myshader.vert.spv");
byte[] fragmentShaderSpirvBytes = File.ReadAllBytes("myshader.vert.spv");
Shader[] shaders = factory.CreateFromSpirv(
    new ShaderDescription(ShaderStages.Vertex, vertexShaderSpirvBytes, "main"),
    new ShaderDescription(ShaderStages.Fragment, vertexShaderSpirvBytes, "main"));
// Use "shaders" array to construct a Pipeline
```

You can also use load GLSL source code and do the same as above. Behind the scenes, Veldrid.SPIRV will compile the GLSL to SPIR-V and then perform the cross-compile to the target language.

## Specialization Constants

Although HLSL and OpenGL-style GLSL do not support SPIR-V Specialization Constants, you can use Veldrid.SPIRV to "specialize" the shader before the target source code is actually emitted. Set `CrossCompileOptions.Specializations` with an array of SpecializationConstant values to accomplish this.

## libveldrid-spirv

Veldrid.SPIRV is implemented primarily as a native library, interfacing with [SPIRV-Cross](https://github.com/KhronosGroup/SPIRV-Cross) and [shaderc](https://github.com/google/shaderc). There are build scripts in the root of the repository which can be used to automatically build the native library for your platform.

Native build requirements:

* CMake
* Python
