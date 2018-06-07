using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Veldrid.SPIRV
{
    public static class SpirvCompilation
    {
        public static unsafe VertexFragmentCompilationResult Compile(
            byte[] vsBytes,
            byte[] fsBytes,
            CompilationTarget target) => Compile(vsBytes, fsBytes, target, Array.Empty<SpecializationConstant>());

        public static unsafe VertexFragmentCompilationResult Compile(
            byte[] vsBytes,
            byte[] fsBytes,
            CompilationTarget target,
            SpecializationConstant[] specializations)
        {
            int size = Unsafe.SizeOf<ShaderSetCompilationInfo>();
            int size2 = Unsafe.SizeOf<SpecializationList>();
            int size3 = Unsafe.SizeOf<SpecializationConstant>();

            ShaderSetCompilationInfo info;
            info.CompilationKind = target;
            fixed (byte* vsBytesPtr = vsBytes)
            fixed (byte* fsBytesPtr = fsBytes)
            fixed (SpecializationConstant* specConstants = specializations)
            {
                info.VertexShader.HasValue = true;
                info.VertexShader.Length = (uint)vsBytes.Length / 4;
                info.VertexShader.ShaderCode = (uint*)vsBytesPtr;

                info.FragmentShader.HasValue = true;
                info.FragmentShader.Length = (uint)fsBytes.Length / 4;
                info.FragmentShader.ShaderCode = (uint*)fsBytesPtr;

                info.Specializations.Count = (uint)specializations.Length;
                info.Specializations.Values = specConstants;

                ShaderCompilationResult* result = null;
                try
                {
                    result = VeldridSpirvNative.Compile(&info);
                    if (!result->Succeeded)
                    {
                        throw new InvalidOperationException("Compilation failed: " + GetString(result->ErrorMessage, result->ErrorMessageLength));
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

        private static unsafe string GetString(byte* data, uint length)
        {
            return Encoding.UTF8.GetString(data, (int)length);
        }
    }
}
