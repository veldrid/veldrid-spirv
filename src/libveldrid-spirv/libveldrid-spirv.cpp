// veldrid-spirv.cpp : Defines the entry point for the console application.
//

#include "libveldrid-spirv.hpp"
#include "InteropStructs.hpp"
#include <fstream>
#include "spirv_hlsl.hpp"
#include "spirv_glsl.hpp"
#include "spirv_msl.hpp"
#include <map>
#include <sstream>
#include "shaderc.hpp"

using namespace spirv_cross;

namespace Veldrid
{

struct BindingInfo
{
    uint32_t Set;
    uint32_t Binding;
};

bool operator <(const BindingInfo& a, const BindingInfo& b)
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
    std::uint32_t IDs[2]; // 0 == VS/CS, 1 == FS
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
            auto bufferBlockFlags = compiler->get_buffer_block_flags(resource.id);
            if (bufferBlockFlags.get(spv::Decoration::DecorationNonWritable))
            {
                return StorageBufferReadOnly;
            }
            else
            {
                return StorageBufferReadWrite;
            }
        }
        else
        {
            return UniformBuffer;
        }
    case SPIRType::BaseType::Image:
        return storage ? StorageImage : SampledImage;
    case SPIRType::BaseType::Sampler:
        return ResourceKind::Sampler;
    default:
        throw std::runtime_error("Unhandled SPIR-V data type.");
    }
}

void AddResources(
    std::vector<spirv_cross::Resource> &resources,
    spirv_cross::Compiler* compiler,
    std::map<BindingInfo, ResourceInfo> &allResources,
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

        auto pair = allResources.insert(std::pair<BindingInfo, ResourceInfo>(bi, ri));
        if (!pair.second)
        {
            pair.first->second.IDs[idIndex] = resource.id;
            if (pair.first->second.Name != resource.name)
            {
                std::stringstream msg;
                msg << "The same binding slot was used by multiple resources: ";
                msg << "\"" << pair.first->second.Name << "\" and ";
                msg << "\"" << resource.name << "\".";
                throw std::runtime_error(msg.str());
            }
        }
    }
}

uint32_t GetResourceIndex(
    CrossCompileTarget target,
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
        if (target == MSL)
        {
            return bufferIndex++;
        }
        else
        {
            return uavIndex++;
        }
    case StorageImage:
        if (target == MSL)
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
        if (target == MSL)
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
        throw std::runtime_error("Invalid ResourceKind.");
    }
}

Compiler* GetCompiler(std::vector<uint32_t> spirvBytes, const CrossCompileInfo& info)
{
    switch (info.Target)
    {
    case HLSL:
    {
        auto ret = new CompilerHLSL(spirvBytes);
        CompilerHLSL::Options opts = {};
        opts.shader_model = 50;
        ret->set_hlsl_options(opts);
        CompilerGLSL::Options commonOpts;
        commonOpts.vertex.flip_vert_y = info.InvertY;
        commonOpts.vertex.fixup_clipspace = info.FixClipSpaceZ;
        ret->set_common_options(commonOpts);
        return ret;
    }
    case GLSL:
    case ESSL:
    {
        auto ret = new CompilerGLSL(spirvBytes);
        CompilerGLSL::Options opts = {};
        opts.es = info.Target == ESSL;
        opts.enable_420pack_extension = false;
        if (info.ComputeShader.Count > 0)
        {
            opts.version = info.Target == GLSL ? 430 : 310;
        }
        else
        {
            opts.version = info.Target == GLSL ? 330 : 300;
        }
        opts.vertex.fixup_clipspace = info.FixClipSpaceZ;
        opts.vertex.flip_vert_y = info.InvertY;
        ret->set_common_options(opts);
        return ret;
    }
    case MSL:
    {
        auto ret = new CompilerMSL(spirvBytes);
        CompilerMSL::Options opts = {};
        ret->set_msl_options(opts);
        CompilerGLSL::Options commonOpts;
        commonOpts.vertex.flip_vert_y = info.InvertY;
        commonOpts.vertex.fixup_clipspace = info.FixClipSpaceZ;
        ret->set_common_options(commonOpts);
        return ret;
    }
    default:
        throw std::runtime_error("Invalid OutputKind.");
    }
}

void SetSpecializations(spirv_cross::Compiler* compiler, const CrossCompileInfo& info)
{
    auto specConstants = compiler->get_specialization_constants();
    for (uint32_t i = 0; i < info.Specializations.Count; i++)
    {
        uint32_t constID = info.Specializations[i].ID;
        uint32_t varID = 0;

        for (auto& constant : specConstants)
        {
            if (constant.constant_id == constID)
            {
                varID = constant.id;
            }
        }

        if (varID != 0)
        {
            auto& constVar = compiler->get_constant(varID);
            constVar.m.c[0].r[0].u64 = info.Specializations[i].Constant;
        }
    }
}

CompilationResult* CompileVertexFragment(const CrossCompileInfo& info)
{
    std::vector<uint32_t> vsBytes(
        info.VertexShader.Data,
        info.VertexShader.Data + info.VertexShader.Count);
    Compiler* vsCompiler = GetCompiler(vsBytes, info);

    std::vector<uint32_t> fsBytes(
        info.FragmentShader.Data,
        info.FragmentShader.Data + info.FragmentShader.Count);
    Compiler* fsCompiler = GetCompiler(fsBytes, info);

    SetSpecializations(vsCompiler, info);
    SetSpecializations(fsCompiler, info);

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

    if (info.Target == HLSL || info.Target == MSL)
    {
        uint32_t bufferIndex = 0;
        uint32_t textureIndex = 0;
        uint32_t uavIndex = 0;
        uint32_t samplerIndex = 0;
        for (auto& it : allResources)
        {
            uint32_t index = GetResourceIndex(info.Target, it.second.Kind, bufferIndex, textureIndex, uavIndex, samplerIndex);

            uint32_t vsID = it.second.IDs[0];
            if (vsID != 0)
            {
                vsCompiler->set_decoration(vsID, spv::Decoration::DecorationBinding, index);
            }
            uint32_t fsID = it.second.IDs[1];
            if (fsID != 0)
            {
                fsCompiler->set_decoration(fsID, spv::Decoration::DecorationBinding, index);
            }
        }
    }

    if (info.Target == GLSL || info.Target == ESSL)
    {
        vsCompiler->build_combined_image_samplers();
        for (auto& remap : vsCompiler->get_combined_image_samplers())
        {
            vsCompiler->set_name(remap.combined_id, vsCompiler->get_name(remap.image_id));
        }

        fsCompiler->build_combined_image_samplers();
        for (auto& remap : fsCompiler->get_combined_image_samplers())
        {
            fsCompiler->set_name(remap.combined_id, fsCompiler->get_name(remap.image_id));
        }

        for (auto& output : vsResources.stage_outputs)
        {
            uint32_t location = vsCompiler->get_decoration(output.id, spv::Decoration::DecorationLocation);
            std::string newName = "vdspv_fsin" + std::to_string(location);
            vsCompiler->set_name(output.id, newName);
        }

        for (auto& input : fsResources.stage_inputs)
        {
            uint32_t location = fsCompiler->get_decoration(input.id, spv::Decoration::DecorationLocation);
            std::string newName = "vdspv_fsin" + std::to_string(location);
            fsCompiler->set_name(input.id, newName);
        }
    }

    if (info.Target == ESSL)
    {
        for (auto& uniformBuffer : vsResources.uniform_buffers)
        {
            vsCompiler->unset_decoration(uniformBuffer.id, spv::Decoration::DecorationBinding);
        }

        uint32_t bufferIndex = 0;
        uint32_t imageIndex = 0;
        for (auto& it : allResources)
        {
            if (it.second.Kind == StorageBufferReadWrite || it.second.Kind == StorageBufferReadWrite)
            {
                uint32_t id = bufferIndex++;
                if (it.second.IDs[0] != 0)
                {
                    vsCompiler->set_decoration(it.second.IDs[0], spv::Decoration::DecorationBinding, id);
                }
                if (it.second.IDs[1] != 0)
                {
                    fsCompiler->set_decoration(it.second.IDs[1], spv::Decoration::DecorationBinding, id);
                }
            }
            else if (it.second.Kind == StorageImage)
            {
                uint32_t id = imageIndex++;
                if (it.second.IDs[0] != 0)
                {
                    vsCompiler->set_decoration(it.second.IDs[0], spv::Decoration::DecorationBinding, id);
                }
                if (it.second.IDs[1] != 0)
                {
                    fsCompiler->set_decoration(it.second.IDs[1], spv::Decoration::DecorationBinding, id);
                }
            }
        }
    }

    std::string vsText = vsCompiler->compile();

    bool usesStorageResource = vsResources.storage_buffers.size() > 0 || vsResources.storage_images.size() > 0;
    if (info.Target == GLSL && usesStorageResource)
    {
        std::string key = "#version 330";
        vsText.replace(vsText.find(key), key.length(), "#version 430");
    }
    else if (info.Target == ESSL && usesStorageResource)
    {
        std::string key = "#version 300";
        vsText.replace(vsText.find(key), key.length(), "#version 310");
    }

    std::string fsText = fsCompiler->compile();

    usesStorageResource = fsResources.storage_buffers.size() > 0 || fsResources.storage_images.size() > 0;
    if (info.Target == GLSL && usesStorageResource)
    {
        std::string key = "#version 330";
        fsText.replace(vsText.find(key), key.length(), "#version 430");
    }
    else if (info.Target == ESSL && usesStorageResource)
    {
        std::string key = "#version 300";
        fsText.replace(vsText.find(key), key.length(), "#version 310");
    }

    delete vsCompiler;
    delete fsCompiler;

    CompilationResult* result = new CompilationResult();
    result->Succeeded = true;

    result->DataBuffers.Resize(2);
    result->DataBuffers[0].CopyFrom(static_cast<uint32_t>(vsText.length()), (uint8_t*)vsText.c_str());
    result->DataBuffers[1].CopyFrom(static_cast<uint32_t>(fsText.length()), (uint8_t*)fsText.c_str());

    return result;
}

CompilationResult* CompileCompute(const CrossCompileInfo& info)
{
    std::vector<uint32_t> csBytes(
        info.ComputeShader.Data,
        info.ComputeShader.Data + info.ComputeShader.Count);
    Compiler* csCompiler = GetCompiler(csBytes, info);

    SetSpecializations(csCompiler, info);

    ShaderResources csResources = csCompiler->get_shader_resources();

    std::map<BindingInfo, ResourceInfo> allResources;

    AddResources(csResources.uniform_buffers, csCompiler, allResources, 0);
    AddResources(csResources.storage_buffers, csCompiler, allResources, 0, false, true);
    AddResources(csResources.separate_images, csCompiler, allResources, 0, true, false);
    AddResources(csResources.storage_images, csCompiler, allResources, 0, true, true);
    AddResources(csResources.separate_samplers, csCompiler, allResources, 0);

    if (info.Target == HLSL || info.Target == MSL)
    {
        uint32_t bufferIndex = 0;
        uint32_t textureIndex = 0;
        uint32_t uavIndex = 0;
        uint32_t samplerIndex = 0;
        for (auto& it : allResources)
        {
            uint32_t index = GetResourceIndex(info.Target, it.second.Kind, bufferIndex, textureIndex, uavIndex, samplerIndex);

            uint32_t csID = it.second.IDs[0];
            if (csID != 0)
            {
                csCompiler->set_decoration(csID, spv::Decoration::DecorationBinding, index);
            }
        }
    }

    if (info.Target == GLSL || info.Target == ESSL)
    {
        csCompiler->build_combined_image_samplers();
        for (auto &remap : csCompiler->get_combined_image_samplers())
        {
            csCompiler->set_name(remap.combined_id, csCompiler->get_name(remap.image_id));
        }
    }

    if (info.Target == ESSL)
    {
        for (auto& uniformBuffer : csResources.uniform_buffers)
        {
            csCompiler->unset_decoration(uniformBuffer.id, spv::Decoration::DecorationBinding);
        }

        uint32_t bufferIndex = 0;
        uint32_t imageIndex = 0;
        for (auto& it : allResources)
        {
            if (it.second.Kind == StorageBufferReadWrite || it.second.Kind == StorageBufferReadWrite)
            {
                csCompiler->set_decoration(it.second.IDs[0], spv::Decoration::DecorationBinding, bufferIndex++);
            }
            else if (it.second.Kind == StorageImage)
            {
                csCompiler->set_decoration(it.second.IDs[0], spv::Decoration::DecorationBinding, imageIndex++);
            }
        }
    }

    std::string csText = csCompiler->compile();

    delete csCompiler;

    CompilationResult* result = new CompilationResult();
    result->Succeeded = true;
    result->DataBuffers.Resize(1);
    result->DataBuffers[0].CopyFrom(static_cast<uint32_t>(csText.length()), (uint8_t*)csText.c_str());

    return result;
}

CompilationResult* Compile(const CrossCompileInfo& info)
{
    if (info.VertexShader.Count > 0 && info.FragmentShader.Count > 0)
    {
        return CompileVertexFragment(info);
    }
    else if (info.ComputeShader.Count > 0)
    {
        return CompileCompute(info);
    }

    return new CompilationResult("The given combination of shaders was not valid.");
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

void WriteToFile(const std::string& path, const std::string& text)
{
    auto outFile = std::ofstream(path);
    outFile << text;
    outFile.close();
}

CompilationResult* CompileGLSLToSPIRV(
    const std::string sourceText,
    shaderc_shader_kind kind,
    std::string fileName,
    const shaderc::CompileOptions& options)
{
    shaderc::Compiler compiler;
    shaderc::SpvCompilationResult result = compiler.CompileGlslToSpv(sourceText, kind, fileName.c_str(), options);

    if (result.GetCompilationStatus() != shaderc_compilation_status_success)
    {
        return new CompilationResult(result.GetErrorMessage());
    }

    uint32_t length = static_cast<uint32_t>(result.end() - result.begin()) * sizeof(uint32_t);
    CompilationResult* ret = new CompilationResult();
    ret->Succeeded = 1;
    ret->DataBuffers.Resize(1);
    ret->DataBuffers[0].CopyFrom(length, (uint8_t*)result.begin());
    return ret;
}

VD_EXPORT CompilationResult* CrossCompile(CrossCompileInfo* info)
{
    try
    {
        return Compile(*info);
    }
    catch (std::exception e)
    {
        return new CompilationResult(e.what());
    }
}

VD_EXPORT CompilationResult* CompileGlslToSpirv(GlslCompileInfo* info)
{
    try
    {
        shaderc::CompileOptions options;

        if (info->Debug)
        {
            options.SetGenerateDebugInfo();
        }
        else
        {
            options.SetOptimizationLevel(shaderc_optimization_level_performance);
        }

        for (uint32_t i = 0; i < info->Macros.Count; i++)
        {
            const MacroDefinition& macro = info->Macros[i];
            if (macro.ValueLength == 0)
            {
                options.AddMacroDefinition(std::string(macro.Name, macro.NameLength));
            }
            else
            {
                options.AddMacroDefinition(
                    std::string(macro.Name, macro.NameLength),
                    std::string(macro.Value, macro.ValueLength));
            }
        }

        return CompileGLSLToSPIRV(
            std::string(info->SourceText.Data, info->SourceText.Count),
            info->Kind,
            std::string(info->FileName.Data, info->FileName.Count),
            options);
    }
    catch (std::exception e)
    {
        return new CompilationResult(e.what());
    }
}

VD_EXPORT void FreeResult(CompilationResult* result)
{
    delete result;
}
}
