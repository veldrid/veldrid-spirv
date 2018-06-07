using System;

namespace Veldrid.SPIRV
{
    public class CompilationOptions
    {
        public bool FixClipSpaceZ { get; set; }
        public bool InvertVertexOutputY { get; set; }
        public SpecializationConstant[] Specializations { get; set; }

        public CompilationOptions()
        {
            Specializations = Array.Empty<SpecializationConstant>();
        }

        public CompilationOptions(bool fixClipSpaceZ, bool invertVertexOutputY, SpecializationConstant[] specializations)
        {
            FixClipSpaceZ = fixClipSpaceZ;
            InvertVertexOutputY = invertVertexOutputY;
            Specializations = specializations;
        }
    }
}
