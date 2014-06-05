using System;
using System.Runtime.Serialization;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Exceptions
{
    public class DispatchFailedException : BusException
    {
        public DispatchFailedException()
        {
        }

        public DispatchFailedException(string message) : base(message)
        {
        }

        public DispatchFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DispatchFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}