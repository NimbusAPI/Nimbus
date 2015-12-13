using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.PropertyInjection;

namespace Nimbus.Interceptors
{
    public class LargeMessageBodyCleanupInterceptor : InboundInterceptor, IRequireLargeMessageBodyStore
    {
        public ILargeMessageBodyStore LargeMessageBodyStore { get; set; }

        public override int Priority
        {
            get { return 0; }
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand buscommand, NimbusMessage nimbusMessage)
        {
            await DeleteAssociatedMessageBody(nimbusMessage);
        }

        private async Task DeleteAssociatedMessageBody(NimbusMessage nimbusMessage)
        {
            object blobIdObject;
            if (!nimbusMessage.Properties.TryGetValue(MessagePropertyKeys.LargeBodyBlobIdentifier, out blobIdObject)) return;

            var blobId = (string) blobIdObject;
            await LargeMessageBodyStore.Delete(blobId);
        }
    }
}