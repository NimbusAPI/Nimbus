using System;
using System.Runtime.Serialization;

namespace Nimbus.Tests.Integration.Tests.ExceptionPropagationTests.RequestHandlers
{
    [Serializable]
    public class DemonstrationException : Exception
    {
        public DemonstrationException()
        {
        }

        public DemonstrationException(string message) : base(message)
        {
        }

        public DemonstrationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DemonstrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}