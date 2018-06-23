using System;
using System.Text;

namespace Veldrid.SPIRV
{
    public static class SpirvCompilation
    {
        public static unsafe VertexFragmentCompilationResult CompileVertexFragment(
            byte[] vsBytes,
            byte[] fsBytes,
            CrossCompileTarget target) => CompileVertexFragment(vsBytes, fsBytes, target, new CrossCompileOptions());

        public static unsafe VertexFragmentCompilationResult CompileVertexFragment(
            byte[] vsBytes,
            byte[] fsBytes,
            CrossCompileTarget target,
            CrossCompileOptions options)
        {
            int size1 = sizeof(CrossCompileInfo);
            int size2 = sizeof(InteropArray);

            byte[] vsSpirvBytes;
            byte[] fsSpirvBytes;

            if (Util.HasSpirvHeader(vsBytes))
            {
                vsSpirvBytes = vsBytes;
            }
            else
            {
                fixed (byte* sourceTextPtr = vsBytes)
                {
                    SpirvCompilationResult vsCompileResult = CompileGlslToSpirv(
                        (uint)vsBytes.Length,
                        sourceTextPtr,
                        string.Empty,
                        ShaderStages.Vertex,
                        false,
                        0,
                        null);
                    vsSpirvBytes = vsCompileResult.SpirvBytes;
                }
            }

            if (Util.HasSpirvHeader(fsBytes))
            {
                fsSpirvBytes = fsBytes;
            }
            else
            {
                fixed (byte* sourceTextPtr = fsBytes)
                {
                    SpirvCompilationResult fsCompileResult = CompileGlslToSpirv(
                        (uint)fsBytes.Length,
                        sourceTextPtr,
                        string.Empty,
                        ShaderStages.Fragment,
                        false,
                        0,
                        null);
                    fsSpirvBytes = fsCompileResult.SpirvBytes;
                }
            }

            CrossCompileInfo info;
            info.Target = target;
            info.FixClipSpaceZ = options.FixClipSpaceZ;
            info.InvertY = options.InvertVertexOutputY;
            fixed (byte* vsBytesPtr = vsSpirvBytes)
            fixed (byte* fsBytesPtr = fsSpirvBytes)
            fixed (SpecializationConstant* specConstants = options.Specializations)
            {
                info.VertexShader = new InteropArray((uint)vsSpirvBytes.Length / 4, vsBytesPtr);
                info.FragmentShader = new InteropArray((uint)fsSpirvBytes.Length / 4, fsBytesPtr);
                info.Specializations = new InteropArray((uint)options.Specializations.Length, specConstants);

                CompilationResult* result = null;
                try
                {
                    result = VeldridSpirvNative.CrossCompile(&info);
                    if (!result->Succeeded)
                    {
                        throw new SpirvCompilationException(
                            "Compilation failed: " + Util.GetString((byte*)result->GetData(0), result->GetLength(0)));
                    }

                    string vsCode = Util.GetString((byte*)result->GetData(0), result->GetLength(0));
                    string fsCode = Util.GetString((byte*)result->GetData(1), result->GetLength(1));
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
            CrossCompileTarget target) => CompileCompute(csBytes, target, new CrossCompileOptions());

        public static unsafe ComputeCompilationResult CompileCompute(
            byte[] csBytes,
            CrossCompileTarget target,
            CrossCompileOptions options)
        {
            byte[] csSpirvBytes;

            if (Util.HasSpirvHeader(csBytes))
            {
                csSpirvBytes = csBytes;
            }
            else
            {
                fixed (byte* sourceTextPtr = csBytes)
                {
                    SpirvCompilationResult vsCompileResult = CompileGlslToSpirv(
                        (uint)csBytes.Length,
                        sourceTextPtr,
                        string.Empty,
                        ShaderStages.Compute,
                        false,
                        0,
                        null);
                    csSpirvBytes = vsCompileResult.SpirvBytes;
                }
            }

            CrossCompileInfo info;
            info.Target = target;
            info.FixClipSpaceZ = options.FixClipSpaceZ;
            info.InvertY = options.InvertVertexOutputY;
            fixed (byte* csBytesPtr = csSpirvBytes)
            fixed (SpecializationConstant* specConstants = options.Specializations)
            {
                info.ComputeShader = new InteropArray((uint)csSpirvBytes.Length / 4, csBytesPtr);
                info.Specializations = new InteropArray((uint)options.Specializations.Length, specConstants);

                CompilationResult* result = null;
                try
                {
                    result = VeldridSpirvNative.CrossCompile(&info);
                    if (!result->Succeeded)
                    {
                        throw new SpirvCompilationException(
                            "Compilation failed: " + Util.GetString((byte*)result->GetData(0), result->GetLength(0)));
                    }

                    string csCode = Util.GetString((byte*)result->GetData(0), result->GetLength(0));
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

        public static unsafe SpirvCompilationResult CompileGlslToSpirv(
            string sourceText,
            string fileName,
            ShaderStages stage,
            GlslCompileOptions options)
        {
            int sourceAsciiCount = Encoding.ASCII.GetByteCount(sourceText);
            byte* sourceAsciiPtr = stackalloc byte[sourceAsciiCount];
            fixed (char* sourceTextPtr = sourceText)
            {
                Encoding.ASCII.GetBytes(sourceTextPtr, sourceText.Length, sourceAsciiPtr, sourceAsciiCount);
            }

            int macroCount = options.Macros.Length;
            NativeMacroDefinition* macros = stackalloc NativeMacroDefinition[(int)macroCount];
            for (int i = 0; i < macroCount; i++)
            {
                macros[i] = new NativeMacroDefinition(options.Macros[i]);
            }

            return CompileGlslToSpirv(
                (uint)sourceAsciiCount,
                sourceAsciiPtr,
                fileName,
                stage,
                options.Debug,
                (uint)macroCount,
                macros);
        }

        internal static unsafe SpirvCompilationResult CompileGlslToSpirv(
            uint sourceLength,
            byte* sourceTextPtr,
            string fileName,
            ShaderStages stage,
            bool debug,
            uint macroCount,
            NativeMacroDefinition* macros)
        {
            GlslCompileInfo info;
            info.Kind = GetShadercKind(stage);
            info.SourceText = new InteropArray(sourceLength, sourceTextPtr);
            info.Debug = debug;
            info.Macros = new InteropArray(macroCount,  macros);

            if (string.IsNullOrEmpty(fileName)) { fileName = "<veldrid-spirv-input>"; }
            int fileNameAsciiCount = Encoding.ASCII.GetByteCount(fileName);
            byte* fileNameAsciiPtr = stackalloc byte[fileNameAsciiCount];
            if (fileNameAsciiCount > 0)
            {
                fixed (char* fileNameTextPtr = fileName)
                {
                    Encoding.ASCII.GetBytes(fileNameTextPtr, fileName.Length, fileNameAsciiPtr, fileNameAsciiCount);
                }
            }
            info.FileName = new InteropArray((uint)fileNameAsciiCount, fileNameAsciiPtr);

            CompilationResult* result = null;
            try
            {
                result = VeldridSpirvNative.CompileGlslToSpirv(&info);
                if (!result->Succeeded)
                {
                    throw new SpirvCompilationException(
                        "Compilation failed: " + Util.GetString((byte*)result->GetData(0), result->GetLength(0)));
                }

                uint length = result->GetLength(0);
                byte[] spirvBytes = new byte[(int)length];
                fixed (byte* spirvBytesPtr = &spirvBytes[0])
                {
                    Buffer.MemoryCopy(result->GetData(0), spirvBytesPtr, length, length);
                }

                return new SpirvCompilationResult(spirvBytes);
            }
            finally
            {
                if (result != null)
                {
                    VeldridSpirvNative.FreeResult(result);
                }
            }
        }

        private static ShadercShaderKind GetShadercKind(ShaderStages stage)
        {
            switch (stage)
            {
                case ShaderStages.Vertex: return ShadercShaderKind.Vertex;
                case ShaderStages.Geometry: return ShadercShaderKind.Geometry;
                case ShaderStages.TessellationControl: return ShadercShaderKind.TessellationControl;
                case ShaderStages.TessellationEvaluation: return ShadercShaderKind.TessellationEvaluation;
                case ShaderStages.Fragment: return ShadercShaderKind.Fragment;
                case ShaderStages.Compute: return ShadercShaderKind.Compute;
                default: throw new SpirvCompilationException($"Invalid shader stage: {stage}");
            }
        }
    }
}
