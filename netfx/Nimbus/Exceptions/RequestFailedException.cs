using System;
using System.Runtime.Serialization;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Exceptions
{
    [Serializable]
    public class RequestFailedException : BusException
    {
        public string ServerStackTrace { get; protected set; }

        public RequestFailedException()
        {
        }

        public RequestFailedException(string message) : base(message)
        {
        }

        public RequestFailedException(string message, string serverStackTrace) : base(message + Environment.NewLine + serverStackTrace)
        {
            ServerStackTrace = serverStackTrace;
        }

        protected RequestFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string ToString()
        {
            return ServerStackTrace + Environment.NewLine + base.ToString();
        }
    }
}