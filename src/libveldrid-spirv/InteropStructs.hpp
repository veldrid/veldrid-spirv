#pragma once

#include "stdint.h"
#include <vector>
#include "spirv_common.hpp"
#include "shaderc.hpp"

namespace Veldrid
{
struct Bool32
{
    uint32_t Value;
    operator bool() const { return Value != 0; }
};

struct ShaderCompilationInfo
{
    Bool32 HasValue;
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

struct SpecializationValue
{
    uint32_t ID;
    uint64_t Constant;
};

struct SpecializationList
{
    uint32_t Count;
    SpecializationValue* Values;
};

enum CompilationTarget
{
    HLSL,
    GLSL,
    ESSL,
    MSL,
};

#pragma pack(push, 1)
struct ShaderSetCompilationInfo
{
    CompilationTarget Target;
    Bool32 FixClipSpaceZ;
    Bool32 InvertY;
    SpecializationList Specializations;
    ShaderCompilationInfo VertexShader;
    ShaderCompilationInfo FragmentShader;
    ShaderCompilationInfo ComputeShader;
};
#pragma pack(pop)
struct ShaderCompilationResult
{
    uint32_t Succeeded;
    uint32_t ErrorMessageLength;
    uint8_t* ErrorMessage;
    uint32_t VertexShaderLength;
    uint8_t* VertexShader;
    uint32_t FragmentShaderLength;
    uint8_t* FragmentShader;
    uint32_t ComputeShaderLength;
    uint8_t* ComputeShader;

    ShaderCompilationResult() { }

    ShaderCompilationResult(const std::string& errorMessage)
    {
        Succeeded = 0;
        ErrorMessageLength = static_cast<uint32_t>(errorMessage.length());
        ErrorMessage = new uint8_t[ErrorMessageLength];
        memcpy(ErrorMessage, errorMessage.c_str(), ErrorMessageLength);

        VertexShaderLength = 0;
        VertexShader = nullptr;

        FragmentShaderLength = 0;
        FragmentShader = nullptr;

        ComputeShaderLength = 0;
        ComputeShader = nullptr;
    }

    ~ShaderCompilationResult()
    {
        if (ErrorMessage != nullptr) { delete[] ErrorMessage; }
        if (VertexShader != nullptr) { delete[] VertexShader; }
        if (FragmentShader != nullptr) { delete[] FragmentShader; }
        if (ComputeShader != nullptr) { delete[] ComputeShader; }
    }
};

#pragma pack(push, 1)
struct GlslCompilationInfo
{
    uint32_t SourceTextLength;
    char* SourceText;
    uint32_t FileNameLength;
    char* FileName;
    shaderc_shader_kind Kind;
};
#pragma pack(pop)
}