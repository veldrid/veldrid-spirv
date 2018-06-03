// veldrid-spirv.cpp : Defines the entry point for the console application.
//

#include "veldrid-spirv.hpp"
#include <fstream>
#include "spirv_hlsl.hpp"
#include "spirv_glsl.hpp"
#include "spirv_msl.hpp"
#include "ShaderSetCompilationInfo.hpp"
#include <map>

using namespace spirv_cross;
using namespace Veldrid;

void WriteToFile(const std::string& path, const std::string& text)
{
    auto outFile = std::ofstream(path);
    outFile << text;
    outFile.close();
}

int main(int argc, char** argv)
{
    auto vsInputPath = std::string(argv[1]);
    auto fsInputPath = std::string(argv[2]);

    auto vsBytes = ReadFile(vsInputPath);
    auto fsBytes = ReadFile(fsInputPath);

    ShaderSetCompilationInfo ssci = {};
    ssci.VertexShader = ShaderCompilationInfo(vsBytes);
    ssci.FragmentShader = ShaderCompilationInfo(fsBytes);
    ssci.CompilationKind = CompilationType::GLSL;
    auto result = Compile(ssci);

    WriteToFile("outvert.hlsl", std::string(result->VertexShader, result->VertexShader + result->VertexShaderLength));
    WriteToFile("outfrag.hlsl", std::string(result->FragmentShader, result->FragmentShader + result->FragmentShaderLength));

    return 0;
}

struct BindingInfo
{
    uint32_t Set;
    uint32_t Binding;
};

bool operator< (const BindingInfo& a, const BindingInfo& b)
{
    return a.Set < b.Set ||
        (a.Set == b.Set && a.Binding < b.Binding);
}

enum ResourceKind
{
    UniformBuffer,
    StorageBufferReadOnly,
    StorageBufferReadWrite,
    SampledImage,
    StorageImage,
    Sampler,
};

struct ResourceInfo
{
    std::string Name;
    ResourceKind Kind;
    std::uint32_t IDs[3]; // 0 == VS, 1 == FS, 2 == CS
};

ResourceKind ClassifyResource(const Compiler* compiler, const Resource& resource, bool image, bool storage)
{
    SPIRType type = compiler->get_type(resource.type_id);
    uint32_t nonWritable = compiler->get_decoration(resource.id, spv::Decoration::DecorationNonWritable);
    switch (type.basetype)
    {
    case SPIRType::BaseType::Struct:
        if (storage)
        {
            return StorageBufferReadOnly;
        }
        else
        {
            return UniformBuffer;
        }
    case SPIRType::BaseType::Image:
        return ResourceKind::SampledImage;
    case SPIRType::BaseType::Sampler:
        return ResourceKind::Sampler;
    }

    throw new std::exception("Unhandled SPIR-V data type.");
}

void AddResources(
    std::vector<spirv_cross::Resource> &resources,
    spirv_cross::Compiler* compiler,
    std::map<BindingInfo, ResourceInfo> &allBuffers,
    const uint32_t idIndex,
    bool image = false,
    bool storage = false)
{
    for (auto& resource : resources)
    {
        ResourceKind kind = ClassifyResource(compiler, resource, image, storage);
        BindingInfo bi;
        bi.Set = compiler->get_decoration(resource.id, spv::Decoration::DecorationDescriptorSet);
        bi.Binding = compiler->get_decoration(resource.id, spv::Decoration::DecorationBinding);

        ResourceInfo ri = {};
        ri.Name = resource.name;
        ri.IDs[idIndex] = resource.id;
        ri.Kind = kind;

        auto pair = allBuffers.insert(std::pair<BindingInfo, ResourceInfo>(bi, ri));
        if (!pair.second)
        {
            pair.first->second.IDs[idIndex] = resource.id;
            if (pair.first->second.Name != resource.name)
            {
                printf("Same binding slot had different names.");
            }
        }
    }
}

uint32_t GetResourceIndex(
    CompilationType outputKind,
    ResourceKind resourceKind,
    uint32_t& bufferIndex,
    uint32_t& textureIndex,
    uint32_t& uavIndex,
    uint32_t& samplerIndex)
{
    switch (resourceKind)
    {
    case UniformBuffer:
        return bufferIndex++;
    case StorageBufferReadWrite:
        if (outputKind == MSL)
        {
            return bufferIndex++;
        }
        else
        {
            return uavIndex++;
        }
    case StorageImage:
        if (outputKind == MSL)
        {
            return textureIndex++;
        }
        else
        {
            return uavIndex++;
        }
    case SampledImage:
        return textureIndex++;
    case StorageBufferReadOnly:
        if (outputKind == MSL)
        {
            return bufferIndex++;
        }
        else
        {
            return textureIndex++;
        }
    case Sampler:
        return samplerIndex++;
    default:
        throw new std::exception("Invalid ResourceKind.");
    }
}

Compiler* GetCompiler(std::vector<uint32_t> spirvBytes, CompilationType kind)
{
    switch (kind)
    {
    case HLSL:
    {
        auto ret = new CompilerHLSL(spirvBytes);
        CompilerHLSL::Options opts = {};
        opts.shader_model = 50;
        ret->set_hlsl_options(opts);
        return ret;
    }
    case GLSL:
    case ESSL:
    {
        auto ret = new CompilerGLSL(spirvBytes);
        CompilerGLSL::Options opts = {};
        opts.es = kind == ESSL;
        opts.enable_420pack_extension = false;
        opts.version = kind == GLSL ? 330 : 300;
        ret->set_common_options(opts);
        return ret;
    }
    case MSL:
    {
        auto ret = new CompilerMSL(spirvBytes);
        CompilerMSL::Options opts = {};
        ret->set_msl_options(opts);
        return ret;
    }
    default:
        throw new std::exception("Invalid OutputKind.");
    }
}

ShaderCompilationResult* CompileVertexFragment(const ShaderSetCompilationInfo& info)
{
    std::vector<uint32_t> vsBytes(
        info.VertexShader.Value.ShaderCode,
        info.VertexShader.Value.ShaderCode + info.VertexShader.Value.Length);
    Compiler* vsCompiler = GetCompiler(vsBytes, info.CompilationKind);

    std::vector<uint32_t> fsBytes(
        info.FragmentShader.Value.ShaderCode,
        info.FragmentShader.Value.ShaderCode + info.FragmentShader.Value.Length);
    Compiler* fsCompiler = GetCompiler(fsBytes, info.CompilationKind);

    if (info.CompilationKind == HLSL || info.CompilationKind == MSL)
    {
        ShaderResources vsResources = vsCompiler->get_shader_resources();
        ShaderResources fsResources = fsCompiler->get_shader_resources();

        std::map<BindingInfo, ResourceInfo> allResources;

        AddResources(vsResources.uniform_buffers, vsCompiler, allResources, 0);
        AddResources(vsResources.storage_buffers, vsCompiler, allResources, 0, false, true);
        AddResources(vsResources.separate_images, vsCompiler, allResources, 0, true, false);
        AddResources(vsResources.storage_images, vsCompiler, allResources, 0, true, true);
        AddResources(vsResources.separate_samplers, vsCompiler, allResources, 0);

        AddResources(fsResources.uniform_buffers, fsCompiler, allResources, 1);
        AddResources(fsResources.storage_buffers, fsCompiler, allResources, 1, false, true);
        AddResources(fsResources.separate_images, fsCompiler, allResources, 1, true, false);
        AddResources(fsResources.storage_images, fsCompiler, allResources, 1, true, true);
        AddResources(fsResources.separate_samplers, fsCompiler, allResources, 1);

        uint32_t bufferIndex = 0;
        uint32_t textureIndex = 0;
        uint32_t uavIndex = 0;
        uint32_t samplerIndex = 0;
        for (auto& it : allResources)
        {
            uint32_t index = GetResourceIndex(info.CompilationKind, it.second.Kind, bufferIndex, textureIndex, uavIndex, samplerIndex);

            uint32_t vsID = it.second.IDs[0];
            if (vsID != 0)
            {
                vsCompiler->set_decoration(it.second.IDs[0], spv::Decoration::DecorationBinding, index);
            }
            uint32_t fsID = it.second.IDs[1];
            if (fsID != 0)
            {
                fsCompiler->set_decoration(it.second.IDs[1], spv::Decoration::DecorationBinding, index);
            }
        }
    }

    if (info.CompilationKind == GLSL || info.CompilationKind == ESSL)
    {
        for (auto& it : vsCompiler->get_shader_resources().uniform_buffers)
        {
            vsCompiler->unset_decoration(it.id, spv::Decoration::DecorationDescriptorSet);
        }

        vsCompiler->build_combined_image_samplers();
        for (auto &remap : vsCompiler->get_combined_image_samplers())
        {
            vsCompiler->set_name(remap.combined_id, vsCompiler->get_name(remap.image_id));
        }

        fsCompiler->build_combined_image_samplers();
        for (auto &remap : fsCompiler->get_combined_image_samplers())
        {
            fsCompiler->set_name(remap.combined_id, fsCompiler->get_name(remap.image_id));
        }
    }

    std::string vsText = vsCompiler->compile();
    std::string fsText = fsCompiler->compile();

    delete vsCompiler;
    delete fsCompiler;

    ShaderCompilationResult* result = new ShaderCompilationResult();
    result->Succeeded = true;
    result->ErrorMessage = nullptr;
    result->ErrorMessageLength = 0;

    uint32_t vsLength = static_cast<uint32_t>(vsText.length());
    result->VertexShader = new uint8_t[vsLength];
    memcpy(result->VertexShader, vsText.c_str(), vsLength);
    result->VertexShaderLength = vsLength;

    uint32_t fsLength = static_cast<uint32_t>(fsText.length());
    result->FragmentShader = new uint8_t[fsLength];
    memcpy(result->FragmentShader, fsText.c_str(), fsLength);
    result->FragmentShaderLength = fsLength;

    return result;
}

ShaderCompilationResult* Compile(const ShaderSetCompilationInfo& info)
{
    if (info.VertexShader.HasValue && info.FragmentShader.HasValue)
    {
        return CompileVertexFragment(info);
    }

    return new ShaderCompilationResult("The given combination of shaders was not valid.");
}

std::vector<uint32_t> ReadFile(std::string path)
{
    std::ifstream is(path, std::ios::binary | std::ios::in | std::ios::ate);
    size_t size = is.tellg();
    is.seekg(0, std::ios::beg);
    char* shaderCode = new char[size];
    is.read(shaderCode, size);
    is.close();

    std::vector<uint32_t> ret(size / 4);
    memcpy(ret.data(), shaderCode, size);

    delete[] shaderCode;
    return ret;
}

VD_EXPORT ShaderCompilationResult* GenerateHLSL(ShaderSetCompilationInfo* info)
{
    try
    {
        return Compile(*info);
    }
    catch (std::exception e)
    {
        return new ShaderCompilationResult(e.what());
    }
}

VD_EXPORT void FreeResult(ShaderCompilationResult* result)
{
    delete result;
}
