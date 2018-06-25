using Xunit;

namespace Veldrid.SPIRV.Tests
{
    public class CompilationTests
    {
        [Theory]
        [InlineData("instance.vert", "instance.frag", CrossCompileTarget.HLSL)]
        [InlineData("instance.vert", "instance.frag", CrossCompileTarget.GLSL)]
        [InlineData("instance.vert", "instance.frag", CrossCompileTarget.ESSL)]
        [InlineData("instance.vert", "instance.frag", CrossCompileTarget.MSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CrossCompileTarget.GLSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CrossCompileTarget.ESSL)]
        [InlineData("instance.vert.spv", "instance.frag.spv", CrossCompileTarget.MSL)]
        [InlineData("instance.vert", "instance.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("instance.vert", "instance.frag.spv", CrossCompileTarget.GLSL)]
        [InlineData("instance.vert", "instance.frag.spv", CrossCompileTarget.ESSL)]
        [InlineData("instance.vert", "instance.frag.spv", CrossCompileTarget.MSL)]
        [InlineData("instance.vert.spv", "instance.frag", CrossCompileTarget.HLSL)]
        [InlineData("instance.vert.spv", "instance.frag", CrossCompileTarget.GLSL)]
        [InlineData("instance.vert.spv", "instance.frag", CrossCompileTarget.ESSL)]
        [InlineData("instance.vert.spv", "instance.frag", CrossCompileTarget.MSL)]
        [InlineData("planet.vert", "planet.frag", CrossCompileTarget.HLSL)]
        [InlineData("planet.vert", "planet.frag", CrossCompileTarget.GLSL)]
        [InlineData("planet.vert", "planet.frag", CrossCompileTarget.ESSL)]
        [InlineData("planet.vert", "planet.frag", CrossCompileTarget.MSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CrossCompileTarget.GLSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CrossCompileTarget.ESSL)]
        [InlineData("planet.vert.spv", "planet.frag.spv", CrossCompileTarget.MSL)]
        [InlineData("starfield.vert", "starfield.frag", CrossCompileTarget.HLSL)]
        [InlineData("starfield.vert", "starfield.frag", CrossCompileTarget.GLSL)]
        [InlineData("starfield.vert", "starfield.frag", CrossCompileTarget.ESSL)]
        [InlineData("starfield.vert", "starfield.frag", CrossCompileTarget.MSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CrossCompileTarget.GLSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CrossCompileTarget.ESSL)]
        [InlineData("starfield.vert.spv", "starfield.frag.spv", CrossCompileTarget.MSL)]
        public void VertexFragmentSucceeds(string vs, string fs, CrossCompileTarget target)
        {
            byte[] vsBytes = TestUtil.LoadBytes(vs);
            byte[] fsBytes = TestUtil.LoadBytes(fs);
            SpecializationConstant[] specializations =
            {
                new SpecializationConstant(100, 125u),
                new SpecializationConstant(101, true),
                new SpecializationConstant(102, 0.75f),
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
        [InlineData("simple.comp", CrossCompileTarget.HLSL)]
        [InlineData("simple.comp", CrossCompileTarget.GLSL)]
        [InlineData("simple.comp", CrossCompileTarget.ESSL)]
        [InlineData("simple.comp", CrossCompileTarget.MSL)]
        [InlineData("simple.comp.spv", CrossCompileTarget.HLSL)]
        [InlineData("simple.comp.spv", CrossCompileTarget.GLSL)]
        [InlineData("simple.comp.spv", CrossCompileTarget.ESSL)]
        [InlineData("simple.comp.spv", CrossCompileTarget.MSL)]
        public void ComputeSucceeds(string cs, CrossCompileTarget target)
        {
            byte[] csBytes = TestUtil.LoadBytes(cs);
            ComputeCompilationResult result = SpirvCompilation.CompileCompute(csBytes, target);
            Assert.NotNull(result.ComputeShader);
        }

        [Theory]
        [InlineData("overlapping-resources.vert.spv", "overlapping-resources.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("overlapping-resources.vert", "overlapping-resources.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("overlapping-resources.vert.spv", "overlapping-resources.frag", CrossCompileTarget.HLSL)]
        [InlineData("overlapping-resources.vert", "overlapping-resources.frag", CrossCompileTarget.HLSL)]
        [InlineData("read-from-buffer.vert", "read-from-buffer.frag", CrossCompileTarget.HLSL)] // SPIRV-Cross Limitation: cannot read struct from ByteAddressBuffer.
        [InlineData("read-from-buffer.vert.spv", "read-from-buffer.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("read-from-buffer.vert", "read-from-buffer.frag.spv", CrossCompileTarget.HLSL)]
        [InlineData("read-from-buffer.vert.spv", "read-from-buffer.frag", CrossCompileTarget.HLSL)]
        public void CompilationFails(string vs, string fs, CrossCompileTarget target)
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
        [InlineData("vertex-gen.comp", CrossCompileTarget.HLSL)] // SPIRV-Cross limitation, see https://github.com/KhronosGroup/SPIRV-Cross/issues/620
        public void CompilationFails_Compute(string cs, CrossCompileTarget target)
        {
            byte[] csBytes = TestUtil.LoadBytes(cs);
            Assert.Throws<SpirvCompilationException>(() =>
               SpirvCompilation.CompileCompute(
                   csBytes,
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
                stage,
                new GlslCompileOptions(
                    false,
                    new MacroDefinition("Name0", "Value0"),
                    new MacroDefinition("Name1", "Value1"),
                    new MacroDefinition("Name2")));

            Assert.NotNull(result.SpirvBytes);
            Assert.True(result.SpirvBytes.Length > 4);
            Assert.True(result.SpirvBytes.Length % 4 == 0);
        }
    }
}
