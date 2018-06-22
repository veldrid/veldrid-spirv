using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CrossCompileInfo
    {
        public CompilationTarget Target;
        public Bool32 FixClipSpaceZ;
        public Bool32 InvertY;
        public SpecializationList Specializations;
        public ShaderData VertexShader;
        public ShaderData FragmentShader;
        public ShaderData ComputeShader;
    }
}
