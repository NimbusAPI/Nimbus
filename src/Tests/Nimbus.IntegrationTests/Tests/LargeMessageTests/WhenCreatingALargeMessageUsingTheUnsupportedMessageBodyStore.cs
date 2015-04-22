﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Tests.Common;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    internal class WhenCreatingALargeMessageUsingTheUnsupportedMessageBodyStore
    {
        protected async Task<BrokeredMessageFactory> Given()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            return new BrokeredMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                              new MaxLargeMessageSizeSetting(),
                                              new MaxSmallMessageSizeSetting {Value = 64*1024},
                                              new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "SomeApp"}, new InstanceNameSetting {Value = "SomeInstance"}),
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
            return await brokeredMessageFactory.Create(bigFatObject);
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException), ExpectedMessage = UnsupportedLargeMessageBodyStore.FailureMessage)]
        public async Task MessageCreationShouldFail()
        {
            var brokeredMessageFactory = await Given();
            var message = await When(brokeredMessageFactory);
        }
    }
}