using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Veldrid.SPIRV
{
    public class VariantStageDescription
    {
        public ShaderStages Stage { get; }
        public string FileName { get; }

        public VariantStageDescription(ShaderStages stage, string fileName)
        {
            Stage = stage;
            FileName = fileName;
        }
    }

    public class ShaderVariantDescription
    {
        public string Name { get; }
        public VariantStageDescription[] Shaders { get; }
        public MacroDefinition[] Macros { get; }
        public CrossCompileOptions CrossCompileOptions { get; }
        public CrossCompileTarget[] Targets { get; }

        public ShaderVariantDescription(
            string name,
            VariantStageDescription[] shaders,
            MacroDefinition[] macros,
            CrossCompileOptions crossCompileOptions,
            CrossCompileTarget[] targets)
        {
            Name = name;
            Shaders = shaders;
            Macros = macros;
            CrossCompileOptions = crossCompileOptions;
            Targets = targets;
        }
    }

    public class VariantCompiler
    {
        private readonly List<string> _shaderSearchPaths = new List<string>();
        private readonly string _outputPath;

        public VariantCompiler(List<string> shaderSearchPaths, string outputPath)
        {
            _shaderSearchPaths = shaderSearchPaths;
            _outputPath = outputPath;
        }

        public void Compile(ShaderVariantDescription variant)
        {
            if (variant.Shaders.Length == 1)
            {
                if (variant.Shaders[0].Stage == ShaderStages.Vertex) { CompileVertexFragment(variant); }
                if (variant.Shaders[0].Stage == ShaderStages.Compute) { CompileCompute(variant); }
                else
                {
                    throw new SpirvCompilationException(
                        $"Variant \"{variant.Name}\" has an unsupported set of shader stages.");
                }
            }
            if (variant.Shaders.Length == 2)
            {
                bool hasVertex = false;
                bool hasFragment = false;
                foreach (var shader in variant.Shaders)
                {
                    hasVertex |= shader.Stage == ShaderStages.Vertex;
                    hasFragment |= shader.Stage == ShaderStages.Fragment;
                }

                if (!hasVertex)
                {
                    throw new SpirvCompilationException($"Variant \"{variant.Name}\" is missing a vertex shader.");
                }
                if (!hasFragment)
                {
                    throw new SpirvCompilationException($"Variant \"{variant.Name}\" is missing a fragment shader.");
                }

                CompileVertexFragment(variant);
            }
        }

        private void CompileVertexFragment(ShaderVariantDescription variant)
        {
            List<Exception> compilationExceptions = new List<Exception>();
            byte[] vsBytes = null;
            byte[] fsBytes = null;

            string vertexFileName = variant.Shaders.FirstOrDefault(vsd => vsd.Stage == ShaderStages.Vertex)?.FileName;
            if (vertexFileName != null)
            {
                try
                {
                    vsBytes = CompileToSpirv(variant, vertexFileName, ShaderStages.Vertex);
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            string fragmentFileName = variant.Shaders.FirstOrDefault(vsd => vsd.Stage == ShaderStages.Fragment)?.FileName;
            if (fragmentFileName != null)
            {
                try
                {
                    fsBytes = CompileToSpirv(variant, fragmentFileName, ShaderStages.Fragment);
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            if (compilationExceptions.Count > 0)
            {
                throw new AggregateException(
                    $"Errors were encountered when compiling from GLSL to SPIR-V.",
                    compilationExceptions);
            }

            foreach (CrossCompileTarget target in variant.Targets)
            {
                try
                {
                    VertexFragmentCompilationResult result = SpirvCompilation.CompileVertexFragment(
                        vsBytes,
                        fsBytes,
                        target,
                        variant.CrossCompileOptions);
                    if (result.VertexShader != null)
                    {
                        string vsPath = Path.Combine(_outputPath, $"{variant.Name}_Vertex.{GetExtension(target)}");
                        File.WriteAllText(vsPath, result.VertexShader);
                    }
                    if (result.FragmentShader != null)
                    {
                        string fsPath = Path.Combine(_outputPath, $"{variant.Name}_Fragment.{GetExtension(target)}");
                        File.WriteAllText(fsPath, result.FragmentShader);
                    }
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            if (compilationExceptions.Count > 0)
            {
                throw new AggregateException($"Errors were encountered when compiling shader variant(s).", compilationExceptions);
            }
        }

        private string GetExtension(CrossCompileTarget target)
        {
            switch (target)
            {
                case CrossCompileTarget.HLSL:
                    return "hlsl";
                case CrossCompileTarget.GLSL:
                    return "glsl";
                case CrossCompileTarget.ESSL:
                    return "essl";
                case CrossCompileTarget.MSL:
                    return "metal";
                default:
                    throw new SpirvCompilationException($"Invalid CrossCompileTarget: {target}");
            }
        }

        private byte[] CompileToSpirv(
            ShaderVariantDescription variant,
            string fileName,
            ShaderStages stage)
        {
            GlslCompileOptions glslOptions = GetOptions(variant);
            string glsl = LoadGlsl(fileName);
            SpirvCompilationResult result = SpirvCompilation.CompileGlslToSpirv(
                glsl,
                fileName,
                stage,
                glslOptions);
            string spvPath = Path.Combine(_outputPath, $"{variant.Name}_{stage.ToString()}.spv");
            File.WriteAllBytes(spvPath, result.SpirvBytes);
            return result.SpirvBytes;
        }

        private GlslCompileOptions GetOptions(ShaderVariantDescription variant)
        {
            return new GlslCompileOptions(true, variant.Macros);
        }

        private string LoadGlsl(string fileName)
        {
            if (fileName == null) { return null; }

            foreach (string searchPath in _shaderSearchPaths)
            {
                string fullPath = Path.Combine(searchPath, fileName);
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
            }

            throw new FileNotFoundException($"Unable to find shader file \"{fileName}\".");
        }

        private void CompileCompute(ShaderVariantDescription variant)
        {
            byte[] csBytes = CompileToSpirv(variant, variant.Shaders[0].FileName, ShaderStages.Compute);

            List<Exception> compilationExceptions = new List<Exception>();
            foreach (CrossCompileTarget target in variant.Targets)
            {
                try
                {
                    ComputeCompilationResult result = SpirvCompilation.CompileCompute(csBytes, target, variant.CrossCompileOptions);
                    string csPath = Path.Combine(_outputPath, $"{variant.Name}_Compute.{GetExtension(target)}");
                    File.WriteAllText(csPath, result.ComputeShader);
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            if (compilationExceptions.Count > 0)
            {
                throw new AggregateException($"Errors were encountered when compiling shader variant(s).", compilationExceptions);
            }
        }
    }
}
