using System;
using System.IO;

namespace Veldrid.SPIRV.Tests
{
    internal static class TestUtil
    {
        public static byte[] LoadShaderBytes(string name)
        {
            return File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestShaders", $"{name}.spv"));
        }
    }
}
