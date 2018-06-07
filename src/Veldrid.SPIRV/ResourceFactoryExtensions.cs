using System;
using System.Text;

namespace Veldrid.SPIRV
{
    public static class ResourceFactoryExtensions
    {
        // TODO: Create return type struct.
        public static (Shader vertexShader, Shader fragmentShader) CreateFromSPIRV(
            this ResourceFactory factory,
            ShaderDescription vertexShaderDescription,
            ShaderDescription fragmentShaderDescription)
            => CreateFromSPIRV(
                factory,
                vertexShaderDescription,
                fragmentShaderDescription,
                new CompilationOptions());

        public static (Shader vertexShader, Shader fragmentShader) CreateFromSPIRV(
            this ResourceFactory factory,
            ShaderDescription vertexShaderDescription,
            ShaderDescription fragmentShaderDescription,
            CompilationOptions options)
        {
            GraphicsBackend backend = factory.BackendType;
            if (backend == GraphicsBackend.Vulkan)
            {
                return (factory.CreateShader(ref vertexShaderDescription), factory.CreateShader(ref fragmentShaderDescription));
            }

            CompilationTarget target = GetCompilationTarget(factory.BackendType);
            VertexFragmentCompilationResult compilationResult = SpirvCompilation.Compile(
                vertexShaderDescription.ShaderBytes,
                fragmentShaderDescription.ShaderBytes,
                target,
                options);

            byte[] vertexBytes = GetBytes(backend, compilationResult.VertexShader);
            Shader vertexShader = factory.CreateShader(new ShaderDescription(
                vertexShaderDescription.Stage,
                vertexBytes,
                vertexShaderDescription.EntryPoint));

            byte[] fragmentBytes = GetBytes(backend, compilationResult.FragmentShader);
            Shader fragmentShader = factory.CreateShader(new ShaderDescription(
                fragmentShaderDescription.Stage,
                fragmentBytes,
                fragmentShaderDescription.EntryPoint));

            return (vertexShader, fragmentShader);
        }

        private static byte[] GetBytes(GraphicsBackend backend, string code)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
                case GraphicsBackend.OpenGL:
                case GraphicsBackend.OpenGLES:
                    return Encoding.ASCII.GetBytes(code);
                case GraphicsBackend.Metal:
                    return Encoding.UTF8.GetBytes(code);
                default:
                    throw new InvalidOperationException($"Invalid GraphicsBackend: {backend}");
            }
        }

        private static CompilationTarget GetCompilationTarget(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
                    return CompilationTarget.HLSL;
                case GraphicsBackend.OpenGL:
                    return CompilationTarget.GLSL;
                case GraphicsBackend.Metal:
                    return CompilationTarget.MSL;
                case GraphicsBackend.OpenGLES:
                    return CompilationTarget.ESSL;
                default:
                    throw new InvalidOperationException($"Invalid GraphicsBackend: {backend}");
            }
        }
    }
}
