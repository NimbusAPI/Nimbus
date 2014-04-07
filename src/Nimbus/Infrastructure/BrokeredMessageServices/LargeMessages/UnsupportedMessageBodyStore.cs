using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages
{
    internal class UnsupportedMessageBodyStore : IMessageBodyStore
    {
        private const string _message = "Your message was too large. Large message support must be configured via BusBuilder.WithLargeMessageBodyStore(...) before use.";

        public Task Store(string id, byte[] bytes)
        {
            throw new NotSupportedException(_message);
        }

        public Task<byte[]> Retrieve(string id)
        {
            throw new NotSupportedException(_message);
        }

        public Task Delete(string id)
        {
            throw new NotSupportedException(_message);
        }
    }
}