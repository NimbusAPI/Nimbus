using System;
using System.Runtime.Serialization;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    [Serializable]
    public class DependencyResolutionException : BusException
    {
        public DependencyResolutionException()
        {
        }

        public DependencyResolutionException(string message) : base(message)
        {
        }

        public DependencyResolutionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DependencyResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}