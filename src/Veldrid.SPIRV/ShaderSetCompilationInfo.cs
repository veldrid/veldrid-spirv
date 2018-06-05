namespace Veldrid.SPIRV
{
    internal struct ShaderSetCompilationInfo
    {
        public CompilationTarget CompilationKind;
        public ShaderCompilationInfo VertexShader;
        public ShaderCompilationInfo FragmentShader;
        public ShaderCompilationInfo ComputeShader;
    };
}
