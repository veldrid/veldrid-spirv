namespace Veldrid.SPIRV
{
    public class VertexFragmentCompilationResult
    {
        public string VertexShader { get; }
        public string FragmentShader { get; }

        internal VertexFragmentCompilationResult(string vertexCode, string fragmentCode)
        {
            VertexShader = vertexCode;
            FragmentShader = fragmentCode;
        }
    }

    public class ComputeCompilationResult
    {
        public string ComputeShader { get; }

        internal ComputeCompilationResult(string computeCode)
        {
            ComputeShader = computeCode;
        }
    }

}
