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
            VertexFragmentCompilationResult result = SpirvCompilation.Compile(vsBytes, fsBytes, target);
            Assert.NotNull(result.VertexShader);
            Assert.NotNull(result.FragmentShader);
        }
    }
}
