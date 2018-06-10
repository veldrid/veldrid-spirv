using System;

namespace Veldrid.SPIRV
{
    public class CompilationOptions
    {
        public InputDepthRange DepthRange { get; set; }
        public bool InvertVertexOutputY { get; set; }
        public SpecializationConstant[] Specializations { get; set; }

        public CompilationOptions()
        {
            Specializations = Array.Empty<SpecializationConstant>();
        }

        public CompilationOptions(InputDepthRange depthRange, bool invertVertexOutputY)
            : this(depthRange, invertVertexOutputY, Array.Empty<SpecializationConstant>())
        {
        }

        public CompilationOptions(InputDepthRange depthRange, bool invertVertexOutputY, SpecializationConstant[] specializations)
        {
            DepthRange = depthRange;
            InvertVertexOutputY = invertVertexOutputY;
            Specializations = specializations;
        }
    }
}
