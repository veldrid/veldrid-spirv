using System;

namespace Veldrid.SPIRV
{
    /// <summary>
    /// An object used to control the options for compiling from GLSL/HLSL to SPIR-V.
    /// </summary>
    public class GlslCompileOptions
    {
        /// <summary>Source language from which to compile. The default is GLSL.</summary>
        public SourceLanguage Language { get; set; }

        /// <summary>
        /// Indicates whether the compiled output should preserve debug information. NOTE: If the resulting SPIR-V is intended to
        /// be used as the source of an OpenGL-style GLSL shader, then this property should be set to <see langword="true"/>.
        /// </summary>
        public bool Debug { get; set; }
        /// <summary>
        /// An array of <see cref="MacroDefinition"/> which defines the set of preprocessor macros to define when compiling the
        /// GLSL source code.
        /// </summary>
        public MacroDefinition[] Macros { get; set; }

        /// <summary>
        /// Gets a default <see cref="GlslCompileOptions"/>.
        /// </summary>
        public static GlslCompileOptions Default { get; } = new GlslCompileOptions();

        /// <summary>
        /// Constructs a new <see cref="GlslCompileOptions"/> with default properties.
        /// </summary>
        public GlslCompileOptions()
        {
            Language = SourceLanguage.GLSL;
            Macros = Array.Empty<MacroDefinition>();
        }

        /// <summary>
        /// Constructs a new <see cref="GlslCompileOptions"/>.
        /// </summary>
        /// <param name="debug">Indicates whether the compiled output should preserve debug information. NOTE: If the resulting
        /// SPIR-V is intended to be used as the source of an OpenGL-style GLSL shader, then this property should be set to
        /// <see langword="true"/>.</param>
        /// <param name="macros">An array of <see cref="MacroDefinition"/> which defines the set of preprocessor macros to define
        /// when compiling the GLSL source code.</param>
        public GlslCompileOptions(bool debug, params MacroDefinition[] macros)
        {
            Language = SourceLanguage.GLSL;
            Debug = debug;
            Macros = macros;
        }
    }
}