using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.RequestResponseTests
{
    [TestFixture]
    internal class WhenAReplyArrivesAfterTheRequestHasBeenRemoved
    {
        [Test]
        public async Task ASingleResponderReplyShouldNotThrow()
        {
            var correlator = new RequestResponseCorrelator(Substitute.For<IClock>(), Substitute.For<ILogger>());
            var requestId = Guid.NewGuid();
            var wrapper = correlator.RecordRequest<string>(requestId, DateTimeOffset.UtcNow.AddMinutes(1));

            correlator.RemoveWrapper(requestId);

            await Should.NotThrowAsync(() => wrapper.Reply("late"));
            await Should.NotThrowAsync(() => wrapper.Throw("late", "stack"));
        }

        [Test]
        public async Task AMulticastReplyShouldNotThrow()
        {
            var correlator = new RequestResponseCorrelator(Substitute.For<IClock>(), Substitute.For<ILogger>());
            var requestId = Guid.NewGuid();
            var wrapper = correlator.RecordMulticastRequest<string>(requestId, DateTimeOffset.UtcNow.AddMinutes(1));

            correlator.RemoveWrapper(requestId);

            await Should.NotThrowAsync(() => wrapper.Reply("late"));
        }
    }
}
