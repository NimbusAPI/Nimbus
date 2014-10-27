using System.Threading.Tasks;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Tests.Common;
using NSubstitute;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    internal abstract class GivenABrokeredMessageFactory : SpecificationForAsync<BrokeredMessageFactory>
    {
        private IClock _clock;
        private ISerializer _serializer;
        protected ReplyQueueNameSetting ReplyQueueNameSetting;

        protected override async Task<BrokeredMessageFactory> Given()
        {
            _clock = Substitute.For<IClock>();
            _serializer = Substitute.For<ISerializer>();

            ReplyQueueNameSetting = new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "TestApplication"}, new InstanceNameSetting {Value = "TestInstance"});

            return new BrokeredMessageFactory(new MaxLargeMessageSizeSetting(),
                                              new MaxSmallMessageSizeSetting(),
                                              ReplyQueueNameSetting,
                                              _clock,
                                              new NullCompressor(),
                                              new DispatchContextManager(),
                                              new UnsupportedLargeMessageBodyStore(),
                                              _serializer,
                                              new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace})
                );
        }
    }
}