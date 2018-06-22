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
        public InteropArray Specializations;
        public InteropArray VertexShader;
        public InteropArray FragmentShader;
        public InteropArray ComputeShader;
    }
}
