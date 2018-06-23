using System;

namespace Veldrid.SPIRV
{
    public class CrossCompileOptions
    {
        public bool FixClipSpaceZ { get; set; }
        public bool InvertVertexOutputY { get; set; }
        public SpecializationConstant[] Specializations { get; set; }

        public CrossCompileOptions()
        {
            Specializations = Array.Empty<SpecializationConstant>();
        }

        public CrossCompileOptions(bool fixClipSpaceZ, bool invertVertexOutputY)
            : this(fixClipSpaceZ, invertVertexOutputY, Array.Empty<SpecializationConstant>())
        {
        }

        public CrossCompileOptions(bool fixClipSpaceZ, bool invertVertexOutputY, params SpecializationConstant[] specializations)
        {
            FixClipSpaceZ = fixClipSpaceZ;
            InvertVertexOutputY = invertVertexOutputY;
            Specializations = specializations;
        }
    }
}
