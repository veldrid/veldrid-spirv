using System;
using System.Runtime.Serialization;

namespace Veldrid.SPIRV
{
    public class SpirvCompilationException : Exception
    {
        public SpirvCompilationException()
        {
        }

        public SpirvCompilationException(string message) : base(message)
        {
        }

        public SpirvCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SpirvCompilationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
