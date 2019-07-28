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
    Bool32() {}
    Bool32(bool value) { Value = value ? 1 : 0; }
};
#pragma pack(pop)

#pragma pack(push, 1)
template <typename T>
struct InteropArray
{
    uint32_t SizeInBytes() const { return Count * sizeof(T); }
    uint32_t Count;
    T *Data;

    T &operator[](uint32_t i) { return Data[i]; }

    const T &operator[](uint32_t i) const { return Data[i]; }

    InteropArray()
    {
        Count = 0;
        Data = nullptr;
    }

    InteropArray(uint32_t count)
    {
        Count = count;
        Data = new T[Count];
    }

    InteropArray(uint32_t count, T *data) { CopyFrom(count, data); }

    InteropArray(const InteropArray &other)
    {
        Count = other.Count;
        Data = new T[Count];
        memcpy(Data, other.Data, Count * sizeof(T));
    }

    InteropArray(InteropArray &&other)
    {
        Count = other.Count;
        Data = other.Data;

        other.Count = 0;
        other.Data = nullptr;
    }

    InteropArray &operator=(InteropArray other)
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

#pragma pack(push, 1)
enum class VertexElementSemantic : uint8_t
{
    Position = 0,
    Normal = 1,
    TextureCoordinate = 2,
    Color = 3
};

enum class VertexElementFormat : uint8_t
{
    Float1 = 0,
    Float2 = 1,
    Float3 = 2,
    Float4 = 3,
    Byte2_Norm = 4,
    Byte2 = 5,
    Byte4_Norm = 6,
    Byte4 = 7,
    SByte2_Norm = 8,
    SByte2 = 9,
    SByte4_Norm = 10,
    SByte4 = 11,
    UShort2_Norm = 12,
    UShort2 = 13,
    UShort4_Norm = 14,
    UShort4 = 15,
    Short2_Norm = 16,
    Short2 = 17,
    Short4_Norm = 18,
    Short4 = 19,
    UInt1 = 20,
    UInt2 = 21,
    UInt3 = 22,
    UInt4 = 23,
    Int1 = 24,
    Int2 = 25,
    Int3 = 26,
    Int4 = 27,
    Half1 = 28,
    Half2 = 29,
    Half4 = 30
};

struct VertexElementDescription
{
    InteropArray<char> Name = InteropArray<char>();
    VertexElementSemantic Semantic = VertexElementSemantic::Position;
    VertexElementFormat Format = VertexElementFormat::Float1;
    uint32_t Offset = 0;
};

enum class ShaderStages : uint8_t
{
    None = 0,
    Vertex = 1,
    Geometry = 2,
    TessellationControl = 4,
    TessellationEvaluation = 8,
    Fragment = 16,
    Compute = 32
};

static inline ShaderStages operator|(ShaderStages left, ShaderStages right)
{
    return static_cast<ShaderStages>(static_cast<uint8_t>(left) | static_cast<uint8_t>(right));
}

enum ResourceKind : uint8_t
{
    UniformBuffer,
    StorageBufferReadOnly,
    StorageBufferReadWrite,
    SampledImage,
    StorageImage,
    Sampler,
};

struct ResourceElementDescription
{
    InteropArray<char> Name;
    ResourceKind Kind;
    ShaderStages Stages;
    uint32_t Options;
};

struct ResourceLayoutDescription
{
    InteropArray<ResourceElementDescription> ResourceElements;
};

struct ReflectionInfo
{
    InteropArray<VertexElementDescription> VertexElements;
    InteropArray<ResourceLayoutDescription> ResourceLayouts;
};

struct CompilationResult
{
    Bool32 Succeeded;
    InteropArray<InteropArray<uint8_t>> DataBuffers;
    ReflectionInfo Reflection;

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
} // namespace Veldrid