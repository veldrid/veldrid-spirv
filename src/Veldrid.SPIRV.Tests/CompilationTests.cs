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
        [InlineData("planet.vert", "planet.frag", CompilationTarget.HLSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.GLSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.ESSL)]
        [InlineData("planet.vert", "planet.frag", CompilationTarget.MSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.HLSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.GLSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.ESSL)]
        [InlineData("starfield.vert", "starfield.frag", CompilationTarget.MSL)]
        public void VertexFragmentSucceeds(string vs, string fs, CompilationTarget target)
        {
            byte[] vsBytes = TestUtil.LoadShaderBytes(vs);
            byte[] fsBytes = TestUtil.LoadShaderBytes(fs);
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
                new CompilationOptions(InputDepthRange.ZeroToOne, false, specializations));
            Assert.NotNull(result.VertexShader);
            Assert.NotNull(result.FragmentShader);
        }

        [Theory]
        [InlineData("simple.comp", CompilationTarget.HLSL)]
        [InlineData("simple.comp", CompilationTarget.GLSL)]
        [InlineData("simple.comp", CompilationTarget.ESSL)]
        [InlineData("simple.comp", CompilationTarget.MSL)]
        public void ComputeSucceeds(string cs, CompilationTarget target)
        {
            byte[] csBytes = TestUtil.LoadShaderBytes(cs);
            ComputeCompilationResult result = SpirvCompilation.CompileCompute(csBytes, target);
            Assert.NotNull(result.ComputeShader);
        }

        [Theory]
        [InlineData("overlapping-resources.vert", "overlapping-resources.frag", CompilationTarget.HLSL)]
        public void CompilationFails(string vs, string fs, CompilationTarget target)
        {
            byte[] vsBytes = TestUtil.LoadShaderBytes(vs);
            byte[] fsBytes = TestUtil.LoadShaderBytes(fs);
            Assert.Throws<SpirvCompilationException>(() =>
               SpirvCompilation.CompileVertexFragment(
                   vsBytes,
                   fsBytes,
                   target,
                   new CompilationOptions(InputDepthRange.ZeroToOne, false)));
        }
    }
}
