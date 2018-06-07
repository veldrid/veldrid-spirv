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
        public void DoTest(string vs, string fs, CompilationTarget target)
        {
            byte[] vsBytes = TestUtil.LoadShaderBytes(vs);
            byte[] fsBytes = TestUtil.LoadShaderBytes(fs);
            SpecializationConstant[] specializations =
            {
                SpecializationConstant.Create(100, 125u),
                SpecializationConstant.Create(101, true),
                SpecializationConstant.Create(102, 0.75f),
            };
            VertexFragmentCompilationResult result = SpirvCompilation.Compile(
                vsBytes,
                fsBytes,
                target,
                new CompilationOptions(false, false, specializations));
            Assert.NotNull(result.VertexShader);
            Assert.NotNull(result.FragmentShader);
        }
    }
}
