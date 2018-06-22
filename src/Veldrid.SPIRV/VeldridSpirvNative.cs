using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    internal static unsafe class VeldridSpirvNative
    {
        private const string LibName = "libveldrid-spirv";

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ShaderCompilationResult* Compile(ShaderSetCompilationInfo* info);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ShaderCompilationResult* CompileGlslToSpirv(GlslCompilationInfo* info);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeResult(ShaderCompilationResult* result);
    }
}
