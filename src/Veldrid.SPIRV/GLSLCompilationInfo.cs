using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct GlslCompilationInfo
    {
        public uint SourceTextLength;
        public byte* SourceText;
        public uint FileNameLength;
        public byte* FileName;
        public ShadercShaderKind Kind;
    };

    internal enum ShadercShaderKind
    {
        Vertex,
        Fragment,
        Compute,
        Geometry,
        TessellationControl,
        TessellationEvaluation,
    }
}
