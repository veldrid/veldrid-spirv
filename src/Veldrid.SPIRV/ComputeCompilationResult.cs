namespace Veldrid.SPIRV
{
    public class ComputeCompilationResult
    {
        public string ComputeShader { get; }

        internal ComputeCompilationResult(string computeCode)
        {
            ComputeShader = computeCode;
        }
    }

}
