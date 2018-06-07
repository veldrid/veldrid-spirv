namespace Veldrid.SPIRV
{
    internal unsafe struct SpecializationList
    {
        public uint Count;
        public SpecializationConstant* Values;
    }
}