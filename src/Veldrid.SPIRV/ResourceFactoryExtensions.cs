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
                vertexShaderDescription.ShaderBytes = EnsureSpirv(vertexShaderDescription);
                fragmentShaderDescription.ShaderBytes = EnsureSpirv(fragmentShaderDescription);

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
                ? "main0"
                : vertexShaderDescription.EntryPoint;
            byte[] vertexBytes = GetBytes(backend, compilationResult.VertexShader);
            Shader vertexShader = factory.CreateShader(new ShaderDescription(
                vertexShaderDescription.Stage,
                vertexBytes,
                vertexEntryPoint));

            string fragmentEntryPoint = (backend == GraphicsBackend.Metal && fragmentShaderDescription.EntryPoint == "main")
                ? "main0"
                : fragmentShaderDescription.EntryPoint;
            byte[] fragmentBytes = GetBytes(backend, compilationResult.FragmentShader);
            Shader fragmentShader = factory.CreateShader(new ShaderDescription(
                fragmentShaderDescription.Stage,
                fragmentBytes,
                fragmentEntryPoint));

            return new Shader[] { vertexShader, fragmentShader };
        }

        public static Shader CreateFromSPIRV(
            this ResourceFactory factory,
            ShaderDescription computeShaderDescription)
        {
            return CreateFromSPIRV(factory, computeShaderDescription, new CrossCompileOptions());
        }

        public static Shader CreateFromSPIRV(
            this ResourceFactory factory,
            ShaderDescription computeShaderDescription,
            CrossCompileOptions options)
        {
            GraphicsBackend backend = factory.BackendType;
            if (backend == GraphicsBackend.Vulkan)
            {
                computeShaderDescription.ShaderBytes = EnsureSpirv(computeShaderDescription);
                return factory.CreateShader(ref computeShaderDescription);
            }

            CrossCompileTarget target = GetCompilationTarget(factory.BackendType);
            ComputeCompilationResult compilationResult = SpirvCompilation.CompileCompute(
                computeShaderDescription.ShaderBytes,
                target,
                options);

            string computeEntryPoint = (backend == GraphicsBackend.Metal && computeShaderDescription.EntryPoint == "main")
                ? "main0"
                : computeShaderDescription.EntryPoint;
            byte[] computeBytes = GetBytes(backend, compilationResult.ComputeShader);
            return factory.CreateShader(new ShaderDescription(
                computeShaderDescription.Stage,
                computeBytes,
                computeEntryPoint));
        }

        private static unsafe byte[] EnsureSpirv(ShaderDescription description)
        {
            if (Util.HasSpirvHeader(description.ShaderBytes))
            {
                return description.ShaderBytes;
            }
            else
            {
                fixed (byte* sourceAsciiPtr = description.ShaderBytes)
                {
                    SpirvCompilationResult glslCompileResult = SpirvCompilation.CompileGlslToSpirv(
                        (uint)description.ShaderBytes.Length,
                        sourceAsciiPtr,
                        null,
                        description.Stage,
                        description.Debug,
                        0,
                        null);
                    return glslCompileResult.SpirvBytes;
                }
            }
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
