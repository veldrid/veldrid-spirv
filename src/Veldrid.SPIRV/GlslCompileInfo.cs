using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct GlslCompileInfo
    {
        /// <summary>
        /// Element type: byte
        /// </summary>
        public InteropArray SourceText;
        /// <summary>
        /// Element type: byte
        /// </summary>
        public InteropArray FileName;
        public ShadercSourceLanguage Language;
        public ShadercShaderKind Kind;
        public Bool32 Debug;
        /// <summary>
        /// Element type: NativeMacroDefinition
        /// </summary>
        public InteropArray Macros;
    };
}
