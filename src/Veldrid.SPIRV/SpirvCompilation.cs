using System.Text;

namespace Veldrid.SPIRV
{
    public static class SpirvCompilation
    {
        public static unsafe VertexFragmentCompilationResult CompileVertexFragment(
            byte[] vsBytes,
            byte[] fsBytes,
            CompilationTarget target) => CompileVertexFragment(vsBytes, fsBytes, target, new CompilationOptions());

        public static unsafe VertexFragmentCompilationResult CompileVertexFragment(
            byte[] vsBytes,
            byte[] fsBytes,
            CompilationTarget target,
            CompilationOptions options)
        {
            ShaderSetCompilationInfo info;
            info.Target = target;
            info.DepthRange = options.DepthRange;
            info.InvertY = options.InvertVertexOutputY;
            fixed (byte* vsBytesPtr = vsBytes)
            fixed (byte* fsBytesPtr = fsBytes)
            fixed (SpecializationConstant* specConstants = options.Specializations)
            {
                info.VertexShader.HasValue = true;
                info.VertexShader.Length = (uint)vsBytes.Length / 4;
                info.VertexShader.ShaderCode = (uint*)vsBytesPtr;

                info.FragmentShader.HasValue = true;
                info.FragmentShader.Length = (uint)fsBytes.Length / 4;
                info.FragmentShader.ShaderCode = (uint*)fsBytesPtr;

                info.Specializations.Count = (uint)options.Specializations.Length;
                info.Specializations.Values = specConstants;

                ShaderCompilationResult* result = null;
                try
                {
                    result = VeldridSpirvNative.Compile(&info);
                    if (!result->Succeeded)
                    {
                        throw new SpirvCompilationException("Compilation failed: " + GetString(result->ErrorMessage, result->ErrorMessageLength));
                    }

                    string vsCode = GetString(result->VertexShader, result->VertexShaderLength);
                    string fsCode = GetString(result->FragmentShader, result->FragmentShaderLength);
                    return new VertexFragmentCompilationResult(vsCode, fsCode);
                }
                finally
                {
                    if (result != null)
                    {
                        VeldridSpirvNative.FreeResult(result);
                    }
                }
            }
        }

        public static unsafe ComputeCompilationResult CompileCompute(
            byte[] csBytes,
            CompilationTarget target) => CompileCompute(csBytes, target, new CompilationOptions());

        public static unsafe ComputeCompilationResult CompileCompute(
            byte[] csBytes,
            CompilationTarget target,
            CompilationOptions options)
        {
            ShaderSetCompilationInfo info;
            info.Target = target;
            info.DepthRange = options.DepthRange;
            info.InvertY = options.InvertVertexOutputY;
            fixed (byte* csBytesPtr = csBytes)
            fixed (SpecializationConstant* specConstants = options.Specializations)
            {
                info.ComputeShader.HasValue = true;
                info.ComputeShader.Length = (uint)csBytes.Length / 4;
                info.ComputeShader.ShaderCode = (uint*)csBytesPtr;

                info.Specializations.Count = (uint)options.Specializations.Length;
                info.Specializations.Values = specConstants;

                ShaderCompilationResult* result = null;
                try
                {
                    result = VeldridSpirvNative.Compile(&info);
                    if (!result->Succeeded)
                    {
                        throw new SpirvCompilationException("Compilation failed: " + GetString(result->ErrorMessage, result->ErrorMessageLength));
                    }

                    string csCode = GetString(result->ComputeShader, result->ComputeShaderLength);
                    return new ComputeCompilationResult(csCode);
                }
                finally
                {
                    if (result != null)
                    {
                        VeldridSpirvNative.FreeResult(result);
                    }
                }
            }
        }

        private static unsafe string GetString(byte* data, uint length)
        {
            return Encoding.UTF8.GetString(data, (int)length);
        }
    }
}
