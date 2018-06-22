using Xunit;

namespace Veldrid.SPIRV.Tests
{
    public class CompilationTests
    {
        [Theory]
        [InlineData("instance.vert", "instance.frag", CompilationTarget.HLSL)]
        [InlineData("instance.vert", "instance.frag", CompilationTarget.GLSL)]
        [InlineData("instance.vert", "instance.frag", CompilationTarget.ESSL)]
        [InlineData("instance.vert", "instance.frag", CompilationTarget.MSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CompilationTarget.HLSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CompilationTarget.GLSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CompilationTarget.ESSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CompilationTarget.MSL)]
        [InlineData("instance.vert", "instance.frag.spv", CompilationTarget.HLSL)]
        [InlineData("instance.vert", "instance.frag.spv", CompilationTarget.GLSL)]
        [InlineData("instance.vert", "instance.frag.spv", CompilationTarget.ESSL)]
        [InlineData("instance.vert", "instance.frag.spv", CompilationTarget.MSL)]
        [InlineData("instance.vert.spv", "instance.frag", CompilationTarget.HLSL)]
        [InlineData("instance.vert.spv", "instance.frag", CompilationTarget.GLSL)]
        [InlineData("instance.vert.spv", "instance.frag", CompilationTarget.ESSL)]
        [InlineData("instance.vert.spv", "instance.frag", CompilationTarget.MSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.HLSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.GLSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.ESSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.MSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CompilationTarget.HLSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CompilationTarget.GLSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CompilationTarget.ESSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CompilationTarget.MSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.HLSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.GLSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.ESSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.MSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CompilationTarget.HLSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CompilationTarget.GLSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CompilationTarget.ESSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CompilationTarget.MSL)]
        public void VertexFragmentSucceeds(string vs, string fs, CompilationTarget target)
        {
            byte[] vsBytes = TestUtil.LoadBytes(vs);
            byte[] fsBytes = TestUtil.LoadBytes(fs);
            SpecializationConstant[] specializations =
            {
                SpecializationConstant.Create(100, 125u),
                SpecializationConstant.Create(101, true),
                SpecializationConstant.Create(102, 0.75f),
            };
            VertexFragmentCompilationResult result = SpirvCompilation.CompileVertexFragment(
                vsBytes,
                fsBytes,
                target,
                new CrossCompileOptions(false, false, specializations));
            Assert.NotNull(result.VertexShader);
            Assert.NotNull(result.FragmentShader);
        }

        [Theory]
        [InlineData("simple.comp", CompilationTarget.HLSL)]
        [InlineData("simple.comp", CompilationTarget.GLSL)]
        [InlineData("simple.comp", CompilationTarget.ESSL)]
        [InlineData("simple.comp", CompilationTarget.MSL)]
        [InlineData("simple.comp.spv", CompilationTarget.HLSL)]
        [InlineData("simple.comp.spv", CompilationTarget.GLSL)]
        [InlineData("simple.comp.spv", CompilationTarget.ESSL)]
        [InlineData("simple.comp.spv", CompilationTarget.MSL)]
        public void ComputeSucceeds(string cs, CompilationTarget target)
        {
            byte[] csBytes = TestUtil.LoadBytes(cs);
            ComputeCompilationResult result = SpirvCompilation.CompileCompute(csBytes, target);
            Assert.NotNull(result.ComputeShader);
        }

        [Theory]
        [InlineData("overlapping-resources.vert.spv", "overlapping-resources.frag.spv", CompilationTarget.HLSL)]
        [InlineData("overlapping-resources.vert", "overlapping-resources.frag.spv", CompilationTarget.HLSL)]
        [InlineData("overlapping-resources.vert.spv", "overlapping-resources.frag", CompilationTarget.HLSL)]
        [InlineData("overlapping-resources.vert", "overlapping-resources.frag", CompilationTarget.HLSL)]
        public void CompilationFails(string vs, string fs, CompilationTarget target)
        {
            byte[] vsBytes = TestUtil.LoadBytes(vs);
            byte[] fsBytes = TestUtil.LoadBytes(fs);
            Assert.Throws<SpirvCompilationException>(() =>
               SpirvCompilation.CompileVertexFragment(
                   vsBytes,
                   fsBytes,
                   target,
                   new CrossCompileOptions(false, false)));
        }

        [Theory]
        [InlineData("instance.vert", ShaderStages.Vertex)]
        [InlineData("instance.frag", ShaderStages.Fragment)]
        [InlineData("planet.vert", ShaderStages.Vertex)]
        [InlineData("planet.frag", ShaderStages.Fragment)]
        [InlineData("starfield.vert", ShaderStages.Vertex)]
        [InlineData("starfield.frag", ShaderStages.Fragment)]
        [InlineData("simple.comp", ShaderStages.Compute)]
        public void GlslToSpirv_Succeeds(string name, ShaderStages stage)
        {
            SpirvCompilationResult result = SpirvCompilation.CompileGlslToSpirv(
                TestUtil.LoadShaderText(name),
                name,
                stage);
            Assert.NotNull(result.SpirvBytes);
            Assert.True(result.SpirvBytes.Length > 4);
            Assert.True(result.SpirvBytes.Length % 4 == 0);
            System.IO.File.WriteAllBytes("outbytes.spv", result.SpirvBytes);
        }
    }
}
