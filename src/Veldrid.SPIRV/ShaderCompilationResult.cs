using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ShaderCompilationResult
    {
        public Bool32 Succeeded;
        public uint ErrorMessageLength;
        public byte* ErrorMessage;
        public uint VertexShaderLength;
        public byte* VertexShader;
        public uint FragmentShaderLength;
        public byte* FragmentShader;
        public uint ComputeShaderLength;
        public byte* ComputeShader;
    }
}
