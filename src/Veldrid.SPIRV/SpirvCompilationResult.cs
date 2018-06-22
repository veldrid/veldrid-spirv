namespace Veldrid.SPIRV
{
    public class SpirvCompilationResult
    {
        public byte[] SpirvBytes { get; }

        public SpirvCompilationResult(byte[] spirvBytes)
        {
            SpirvBytes = spirvBytes;
        }
    }
}
