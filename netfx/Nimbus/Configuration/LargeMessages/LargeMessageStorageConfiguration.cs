using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.LargeMessages
{
    public abstract class LargeMessageStorageConfiguration : INimbusConfiguration
    {
        public MaxSmallMessageSizeSetting MaxSmallMessageSize { get; set; } = new MaxSmallMessageSizeSetting();
        public MaxLargeMessageSizeSetting MaxLargeMessageSize { get; set; } = new MaxLargeMessageSizeSetting();

        public LargeMessageStorageConfiguration WithMaxSmallMessageSize(int messageSize)
        {
            MaxSmallMessageSize = new MaxSmallMessageSizeSetting {Value = messageSize};
            return this;
        }

        public LargeMessageStorageConfiguration WithMaxLargeMessageSize(int messageSize)
        {
            MaxLargeMessageSize = new MaxLargeMessageSizeSetting {Value = messageSize};
            return this;
        }

        public void RegisterWith(PoorMansIoC container)
        {
            RegisterSupportingComponents(container);
            Register<ILargeMessageBodyStore>(container);
        }

        public abstract void Register<TLargeMessageBodyStore>(PoorMansIoC container) where TLargeMessageBodyStore : ILargeMessageBodyStore;
        public abstract void RegisterSupportingComponents(PoorMansIoC container);
        public abstract IEnumerable<string> Validate();
    }
}