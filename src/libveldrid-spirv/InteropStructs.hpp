#pragma once

#include "stdint.h"
#include <vector>
#include "spirv_common.hpp"
#include "shaderc.hpp"
#include <assert.h>

namespace Veldrid
{
struct Bool32
{
    uint32_t Value;
    operator bool() const { return Value != 0; }
};

struct ShaderData
{
    Bool32 HasValue;
    uint32_t Length;
    uint32_t* ShaderCode; // SPIR-V bytecode

    ShaderData(std::vector<uint32_t>& vec)
    {
        ShaderCode = vec.data();
        Length = static_cast<uint32_t>(vec.size());
    }

    ShaderData()
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

enum CrossCompileTarget
{
    HLSL,
    GLSL,
    ESSL,
    MSL,
};

#pragma pack(push, 1)
struct CrossCompileInfo
{
    CrossCompileTarget Target;
    Bool32 FixClipSpaceZ;
    Bool32 InvertY;
    SpecializationList Specializations;
    ShaderData VertexShader;
    ShaderData FragmentShader;
    ShaderData ComputeShader;
};
#pragma pack(pop)
struct CompilationResult
{
    uint32_t Succeeded;
    uint32_t DataBufferCount;
    uint32_t* DataBufferLengths;
    void** DataBuffers;

    CompilationResult()
    {
        Succeeded = 0;
        DataBufferCount = 0;
        DataBufferLengths = nullptr;
        DataBuffers = nullptr;
    }

    CompilationResult(const std::string& errorMessage)
    {
        Succeeded = 0;
        DataBufferCount = 1;
        DataBuffers = new void*[1];
        DataBufferLengths = new uint32_t[1];
        size_t errorLength = errorMessage.length();
        DataBufferLengths[0] = static_cast<uint32_t>(errorLength);
        DataBuffers[0] = new uint8_t[errorLength];
        memcpy(DataBuffers[0], errorMessage.c_str(), errorLength);
    }

    ~CompilationResult()
    {
        for (uint32_t i = 0; i < DataBufferCount; i++)
        {
            delete[] DataBuffers[i];
        }

        delete[] DataBuffers;
        delete[] DataBufferLengths;
    }

    void SetDataBufferCount(uint32_t count)
    {
        assert(DataBufferLengths == nullptr && DataBuffers == nullptr);
        DataBufferCount = count;
        DataBufferLengths = new uint32_t[count];
        DataBuffers = new void*[count];
    }

    void SetData(uint32_t index, uint32_t dataSize, const void* data)
    {
        assert(DataBufferCount > index);
        DataBufferLengths[index] = dataSize;
        DataBuffers[index] = new void*[dataSize];
        memcpy(DataBuffers[index], data, dataSize);
    }
};

#pragma pack(push, 1)
struct GlslCompileInfo
{
    uint32_t SourceTextLength;
    char* SourceText;
    uint32_t FileNameLength;
    char* FileName;
    shaderc_shader_kind Kind;
};
#pragma pack(pop)
}