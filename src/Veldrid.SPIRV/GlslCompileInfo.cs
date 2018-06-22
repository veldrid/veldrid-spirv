using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct GlslCompileInfo
    {
        public uint SourceTextLength;
        public byte* SourceText;
        public uint FileNameLength;
        public byte* FileName;
        public ShadercShaderKind Kind;
        public Bool32 Debug;
        public uint MacroCount;
        public NativeMacroDefinition* Macros;
    };
}
