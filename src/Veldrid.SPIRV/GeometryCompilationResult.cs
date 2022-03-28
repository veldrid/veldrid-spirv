namespace Veldrid.SPIRV
{
    /// <summary>
    /// The output of a cross-compile operation of a compute shader from SPIR-V to some target language.
    /// </summary>
    public class GeometryCompilationResult
    {
        /// <summary>
        /// The translated shader source code.
        /// </summary>
        public string GeometryShader { get; }
        /// <summary>
        /// Information about the resources used in the compiled shader.
        /// </summary>
        public SpirvReflection Reflection { get; }

        internal GeometryCompilationResult(string geometryCode, SpirvReflection reflection)
        {
            this.GeometryShader = geometryCode;
            Reflection = reflection;
        }
    }

}
