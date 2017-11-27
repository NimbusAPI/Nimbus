using System;
using System.Runtime.Serialization;

namespace Nimbus.MessageContracts.Exceptions
{
    [Serializable]
    public class BusException : Exception
    {
        public BusException()
        {
        }

        public BusException(string message) : base(message)
        {
        }

        public BusException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}