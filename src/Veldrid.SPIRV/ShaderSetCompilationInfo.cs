using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ShaderSetCompilationInfo
    {
        public CompilationTarget Target;
        public InputDepthRange DepthRange;
        public Bool32 InvertY;
        public SpecializationList Specializations;
        public ShaderCompilationInfo VertexShader;
        public ShaderCompilationInfo FragmentShader;
        public ShaderCompilationInfo ComputeShader;
    }
}
