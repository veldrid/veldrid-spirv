using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ShaderSetCompilationInfo
    {
        public CompilationTarget Target;
        public Bool32 FixClipSpaceZ;
        public Bool32 InvertY;
        public SpecializationList Specializations;
        public ShaderCompilationInfo VertexShader;
        public ShaderCompilationInfo FragmentShader;
        public ShaderCompilationInfo ComputeShader;
    };

    public unsafe struct SpecializationConstant
    {
        internal uint ID;
        internal ulong Constant;

        public static SpecializationConstant Create(uint id, bool value) => CreateT(id, value ? 1u : 0u);
        public static SpecializationConstant Create(uint id, uint value) => CreateT(id, value);
        public static SpecializationConstant Create(uint id, int value) => CreateT(id, value);
        public static SpecializationConstant Create(uint id, ulong value) => CreateT(id, value);
        public static SpecializationConstant Create(uint id, long value) => CreateT(id, value);
        public static SpecializationConstant Create(uint id, float value) => CreateT(id, value);
        public static SpecializationConstant Create(uint id, double value) => CreateT(id, value);

        internal static SpecializationConstant CreateT<T>(uint id, T value)
        {
            SpecializationConstant ret;
            ret.ID = id;
            Unsafe.Write(&ret.Constant, value);
            return ret;
        }
    };

    internal unsafe struct SpecializationList
    {
        public uint Count;
        public SpecializationConstant* Values;
    };
}
