using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SpecializationConstant
    {
        public uint ID;
        public ulong Constant;

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
    }
}