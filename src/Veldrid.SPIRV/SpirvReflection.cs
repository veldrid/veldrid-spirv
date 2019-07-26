namespace Veldrid.SPIRV
{
    public class SpirvReflection
    {
        public VertexElementDescription[] VertexElements { get; }
        public ResourceLayoutDescription[] ResourceLayouts { get; }

        internal SpirvReflection(
            VertexElementDescription[] vertexElements,
            ResourceLayoutDescription[] resourceLayouts)
        {
            VertexElements = vertexElements;
            ResourceLayouts = resourceLayouts;
        }
    }
}