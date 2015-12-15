using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.NimbusMessageServices;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    internal class WhenCreatingALargeMessageUsingTheUnsupportedMessageBodyStore
    {
        protected async Task<NimbusMessageFactory> Given()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            return new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                              new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "SomeApp"}, new InstanceNameSetting {Value = "SomeInstance"}),
                                              new SystemClock(),
                                              new DispatchContextManager());
        }

        private async Task<NimbusMessage> When(NimbusMessageFactory brokeredMessageFactory)
        {
            var bigFatObject = new string(Enumerable.Range(0, 256*1024).Select(i => '.').ToArray());
            return await brokeredMessageFactory.Create(bigFatObject);
        }

        [Test]
        public async Task MessageCreationShouldFail()
        {
            var brokeredMessageFactory = await Given();
            Should.Throw<NotSupportedException>(() => When(brokeredMessageFactory).Wait());
        }
    }
}