using System;
using System.Text;

namespace Veldrid.SPIRV
{
    public static class ResourceFactoryExtensions
    {
        public static Shader[] CreateFromSPIRV(
            this ResourceFactory factory,
            ShaderDescription vertexShaderDescription,
            ShaderDescription fragmentShaderDescription)
        {
            return CreateFromSPIRV(factory, vertexShaderDescription, fragmentShaderDescription, new CrossCompileOptions());
        }

        public static Shader[] CreateFromSPIRV(
            this ResourceFactory factory,
            ShaderDescription vertexShaderDescription,
            ShaderDescription fragmentShaderDescription,
            CrossCompileOptions options)
        {
            GraphicsBackend backend = factory.BackendType;
            if (backend == GraphicsBackend.Vulkan)
            {
                return new Shader[]
                {
                    factory.CreateShader(ref vertexShaderDescription),
                    factory.CreateShader(ref fragmentShaderDescription)
                };
            }

            CrossCompileTarget target = GetCompilationTarget(factory.BackendType);
            VertexFragmentCompilationResult compilationResult = SpirvCompilation.CompileVertexFragment(
                vertexShaderDescription.ShaderBytes,
                fragmentShaderDescription.ShaderBytes,
                target,
                options);

            string vertexEntryPoint = (backend == GraphicsBackend.Metal && vertexShaderDescription.EntryPoint == "main")
                ? vertexEntryPoint = "main0"
                : vertexEntryPoint = vertexShaderDescription.EntryPoint;
            byte[] vertexBytes = GetBytes(backend, compilationResult.VertexShader);
            Shader vertexShader = factory.CreateShader(new ShaderDescription(
                vertexShaderDescription.Stage,
                vertexBytes,
                vertexEntryPoint));

            string fragmentEntryPoint = (backend == GraphicsBackend.Metal && fragmentShaderDescription.EntryPoint == "main")
                ? fragmentEntryPoint = "main0"
                : fragmentEntryPoint = fragmentShaderDescription.EntryPoint;
            byte[] fragmentBytes = GetBytes(backend, compilationResult.FragmentShader);
            Shader fragmentShader = factory.CreateShader(new ShaderDescription(
                fragmentShaderDescription.Stage,
                fragmentBytes,
                fragmentEntryPoint));

            return new Shader[] { vertexShader, fragmentShader };
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
                    throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}");
            }
        }

        private static CrossCompileTarget GetCompilationTarget(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
                    return CrossCompileTarget.HLSL;
                case GraphicsBackend.OpenGL:
                    return CrossCompileTarget.GLSL;
                case GraphicsBackend.Metal:
                    return CrossCompileTarget.MSL;
                case GraphicsBackend.OpenGLES:
                    return CrossCompileTarget.ESSL;
                default:
                    throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}");
            }
        }
    }
}
