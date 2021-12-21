using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CrossCompileInfo
    {
        public CrossCompileTarget Target;
        public uint TargetVersion;
        public Bool32 FixClipSpaceZ;
        public Bool32 InvertY;
        public Bool32 NormalizeResourceNames;
        public InteropArray Specializations;
        public InteropArray VertexShader;
        public InteropArray FragmentShader;
        public InteropArray ComputeShader;
    }
}
