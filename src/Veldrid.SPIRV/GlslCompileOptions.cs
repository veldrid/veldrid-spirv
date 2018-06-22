using System;

namespace Veldrid.SPIRV
{
    public class GlslCompileOptions
    {
        public bool Debug { get; set; }
        public MacroDefinition[] Macros { get; set; }

        public static GlslCompileOptions Default { get; } = new GlslCompileOptions();

        public GlslCompileOptions()
        {
            Macros = Array.Empty<MacroDefinition>();
        }

        public GlslCompileOptions(bool debug, params MacroDefinition[] macros)
        {
            Debug = debug;
            Macros = macros;
        }
    }
}