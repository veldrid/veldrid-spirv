#pragma once

#include "stdint.h"
#include <vector>
#include "spirv_common.hpp"
#include "shaderc.hpp"
#include <assert.h>

namespace Veldrid
{
#pragma pack(push, 1)
struct Bool32
{
    uint32_t Value;
    operator bool() const { return Value != 0; }
    Bool32() { }
    Bool32(bool value) { Value = value ? 1 : 0; }
};
#pragma pack(pop)

#pragma pack(push, 1)
template<typename T>
struct InteropArray
{
    uint32_t SizeInBytes() const { return Count * sizeof(T); }
    uint32_t Count;
    T* Data;

    T& operator [](uint32_t i) { return Data[i]; }

    const T& operator [](uint32_t i) const { return Data[i]; }

    InteropArray()
    {
        Count = 0;
        Data = nullptr;
    }

    InteropArray(uint32_t count, T* data) { CopyFrom(count, data); }

    InteropArray(const InteropArray& other)
    {
        Count = other.Count;
        Data = new T[Count];
        memcpy(Data, other.Data, Count * sizeof(T));
    }

    InteropArray(InteropArray&& other)
    {
        Count = other.Count;
        Data = other.Data;

        other.Count = 0;
        other.Data = nullptr;
    }

    InteropArray& operator=(InteropArray other)
    {
        Count = other.Count;
        std::swap(Data, other.Data);
        return *this;
    }

    ~InteropArray()
    {
        if (Data != nullptr)
        {
            delete[] Data;
        }
    }

    void Resize(uint32_t newCount)
    {
        if (Data != nullptr)
        {
            delete[] Data;
        }

        Count = newCount;
        Data = new T[newCount];
    }

    void CopyFrom(uint32_t count, const T* data)
    {
        if (Data != nullptr)
        {
            delete[] Data;
        }

        Count = count;
        Data = new T[count];
        memcpy(Data, data, count * sizeof(T));
    }
};
#pragma pack(pop)

#pragma pack(push, 1)
struct SpecializationConstant
{
    uint32_t ID;
    uint64_t Constant;
};
#pragma pack(pop)

#pragma pack(push, 1)
enum CrossCompileTarget : uint32_t
{
    HLSL,
    GLSL,
    ESSL,
    MSL,
};
#pragma pack(pop)

#pragma pack(push, 1)
struct CrossCompileInfo
{
    CrossCompileTarget Target;
    Bool32 FixClipSpaceZ;
    Bool32 InvertY;
    InteropArray<SpecializationConstant> Specializations;
    InteropArray<uint32_t> VertexShader;
    InteropArray<uint32_t> FragmentShader;
    InteropArray<uint32_t> ComputeShader;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct CompilationResult
{
    Bool32 Succeeded;
    InteropArray<InteropArray<uint8_t>> DataBuffers;

    CompilationResult()
    {
        Succeeded.Value = 0;
        DataBuffers.Count = 0;
        DataBuffers.Data = nullptr;
    }

    CompilationResult(const std::string& errorMessage)
    {
        Succeeded.Value = 0;
        DataBuffers.Resize(1);
        DataBuffers[0].CopyFrom(static_cast<uint32_t>(errorMessage.length()), (uint8_t*)(errorMessage.c_str()));
    }
};
#pragma pack(pop)

#pragma pack(push, 1)
struct MacroDefinition
{
    uint32_t NameLength;
    char Name[128];
    uint32_t ValueLength;
    char Value[128];
};
#pragma pack(pop)

#pragma pack(push, 1)
struct GlslCompileInfo
{
    InteropArray<char> SourceText;
    InteropArray<char> FileName;
    shaderc_shader_kind Kind;
    Bool32 Debug;
    InteropArray<MacroDefinition> Macros;
};
#pragma pack(pop)
}