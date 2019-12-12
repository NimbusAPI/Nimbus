using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.LargeMessages
{
    public class UnsupportedLargeMessageBodyStorageConfiguration : LargeMessageStorageConfiguration
    {
        public override void Register<TLargeMessageBodyStore>(PoorMansIoC container)
        {
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));
        }

        public override void RegisterSupportingComponents(PoorMansIoC container)
        {
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}