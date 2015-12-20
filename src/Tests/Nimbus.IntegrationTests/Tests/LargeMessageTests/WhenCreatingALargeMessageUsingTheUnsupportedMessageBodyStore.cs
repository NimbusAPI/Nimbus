using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.NimbusMessageServices.LargeMessages;
using Nimbus.Tests.Common;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    internal class WhenCreatingALargeMessageUsingTheUnsupportedMessageBodyStore
    {
        protected async Task<BrokeredMessageFactory> Given()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            return new BrokeredMessageFactory(
                new MaxLargeMessageSizeSetting(),
                new MaxSmallMessageSizeSetting {Value = 64*1024},
                new SystemClock(),
                new NullCompressor(),
                new DispatchContextManager(),
                new UnsupportedLargeMessageBodyStore(),
                new DataContractSerializer(typeProvider),
                typeProvider);
        }

        private async Task<BrokeredMessage> When(BrokeredMessageFactory brokeredMessageFactory)
        {
            var bigFatObject = new string(Enumerable.Range(0, 256*1024).Select(i => '.').ToArray());
            var nimbusMessage = new NimbusMessage("noPath", bigFatObject);
            return await brokeredMessageFactory.BuildBrokeredMessage(nimbusMessage);
        }

        [Test]
        public async Task MessageCreationShouldFail()
        {
            var brokeredMessageFactory = await Given();
            try
            {
                await When(brokeredMessageFactory);
                Assert.Fail();
            }
            catch (Exception e)
            {
                e.ShouldBeTypeOf<NotSupportedException>();
            }
        }
    }
}