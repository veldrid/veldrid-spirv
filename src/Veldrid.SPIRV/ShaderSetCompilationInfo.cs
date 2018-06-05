namespace Veldrid.SPIRV
{
    internal struct ShaderSetCompilationInfo
    {
        public CompilationTarget CompilationKind;
        public Bool32 InvertY;
        public ShaderCompilationInfo VertexShader;
        public ShaderCompilationInfo FragmentShader;
        public ShaderCompilationInfo ComputeShader;
    };
}
