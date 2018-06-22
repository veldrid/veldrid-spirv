using System;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct CompilationResult
    {
        public Bool32 Succeeded;
        public uint DataBufferCount;
        public uint* DataBufferLengths;
        public void** DataBuffers;

        public uint GetLength(uint index)
        {
            if (index >= DataBufferCount) { throw new ArgumentOutOfRangeException(nameof(index)); }
            return DataBufferLengths[index];
        }

        public void* GetData(uint index)
        {
            if (index >= DataBufferCount) { throw new ArgumentOutOfRangeException(nameof(index)); }
            return DataBuffers[index];
        }
    }
}
