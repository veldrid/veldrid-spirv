#pragma once

#include "stdint.h"
#include <vector>

namespace Veldrid
{
template<class T>
struct Optional
{
    bool HasValue;
    T Value;

    Optional(T value)
    {
        HasValue = true;
        Value = value;
    }

    Optional()
    {
        HasValue = false;
    }
};
struct ShaderCompilationInfo
{
    uint32_t Length;
    uint32_t* ShaderCode; // SPIR-V bytecode

    ShaderCompilationInfo(std::vector<uint32_t>& vec)
    {
        ShaderCode = vec.data();
        Length = static_cast<uint32_t>(vec.size());
    }

    ShaderCompilationInfo()
    {
        ShaderCode = nullptr;
        Length = 0;
    }
};
struct ShaderSetCompilationInfo
{
    Optional<ShaderCompilationInfo> VertexShader;
    Optional<ShaderCompilationInfo> FragmentShader;
    Optional<ShaderCompilationInfo> ComputeShader;
};
struct ShaderCompilationResult
{
    bool Succeeded;
    uint32_t VertexShaderLength;
    uint8_t* VertexShader;
    uint32_t FragmentShaderLength;
    uint8_t* FragmentShader;
    uint32_t ComputeShaderLength;
    uint8_t* ComputeShader;
};
}