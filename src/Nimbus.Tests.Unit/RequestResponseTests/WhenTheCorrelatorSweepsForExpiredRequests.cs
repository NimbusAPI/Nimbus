using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.RequestResponseTests
{
    /// <summary>
    ///     Regression tests for https://github.com/NimbusAPI/Nimbus/issues/317.
    /// </summary>
    [TestFixture]
    internal class WhenTheCorrelatorSweepsForExpiredRequests
    {
        private const int SweepThreshold = 1000;

        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

        private RequestResponseCorrelator _correlator;
        private Guid _expiredRequestId;
        private Guid _liveRequestId;
        private RequestResponseCorrelationWrapper<string> _liveWrapper;

        [SetUp]
        public async Task SetUp()
        {
            var clock = Substitute.For<IClock>();
            clock.UtcNow.Returns(Now);
            _correlator = new RequestResponseCorrelator(clock, Substitute.For<ILogger>());

            _expiredRequestId = Guid.NewGuid();
            _correlator.RecordRequest<string>(_expiredRequestId, Now.AddMinutes(-1));

            _liveRequestId = Guid.NewGuid();
            _liveWrapper = _correlator.RecordRequest<string>(_liveRequestId, Now.AddHours(1));

            // Fill up to the threshold with more live requests; the last one triggers the sweep.
            foreach (var _ in Enumerable.Range(0, SweepThreshold - 2))
            {
                _correlator.RecordRequest<string>(Guid.NewGuid(), Now.AddHours(1));
            }

            // The sweep runs on a background task.
            var sw = Stopwatch.StartNew();
            while (_correlator.TryGetWrapper(_expiredRequestId) != null && sw.Elapsed < TimeSpan.FromSeconds(2))
            {
                await Task.Delay(10);
            }
        }

        [Test]
        public void TheExpiredRequestShouldBeRemoved()
        {
            _correlator.TryGetWrapper(_expiredRequestId).ShouldBeNull();
        }

        [Test]
        public void TheLiveRequestShouldStillBeTracked()
        {
            _correlator.TryGetWrapper(_liveRequestId).ShouldBeSameAs(_liveWrapper);
        }

        [Test]
        public async Task TheLiveRequestShouldStillBeAbleToReceiveAResponse()
        {
            await _liveWrapper.Reply("Hello");
            (await _liveWrapper.WaitForResponse(TimeSpan.FromSeconds(1))).ShouldBe("Hello");
        }
    }
}
