using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors
{
    internal class LargeMessageBodyCleanupInterceptor : ICommandInterceptor<IBusCommand>
    {
        private readonly ILargeMessageBodyStore _largeMessageBodyStore;

        public LargeMessageBodyCleanupInterceptor(ILargeMessageBodyStore largeMessageBodyStore)
        {
            _largeMessageBodyStore = largeMessageBodyStore;
        }

        public int Priority
        {
            get { return 0; }
        }

        public async Task OnHandlerExecuting(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
        }

        public async Task OnHandlerSuccess(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            object blobIdObject;
            if (!brokeredMessage.Properties.TryGetValue(MessagePropertyKeys.LargeBodyBlobIdentifier, out blobIdObject)) return;

            var blobId = blobIdObject as string;
            await _largeMessageBodyStore.Delete(blobId);
        }

        public async Task OnHandlerError(IBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
        {
        }
    }
}