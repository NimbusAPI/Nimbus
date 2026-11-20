using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.MessageContracts;
using Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.RequestHandlers;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests
{
    /// <summary>
    ///     Regression test for https://github.com/NimbusAPI/Nimbus/issues/317. The correlator sweeps for expired requests
    ///     every 1,000 requests; it must not remove or dispose requests that are still waiting for a response.
    /// </summary>
    [TestFixture]
    public class WhenMoreThanAThousandRequestsAreSentWhileOthersAreInFlight
    {
        private const int SlowRequestCount = 10;
        private const int FastRequestCount = 1000;
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

        private Bus _bus;
        private Exception[] _slowRequestFailures;
        private Exception[] _fastRequestFailures;
        private TimeSpan _slowRequestElapsed;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            SlowRequestHandler.Gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            SlowRequestHandler.Received = 0;

            var testFixtureType = GetType();
            var typeProvider = new TestHarnessTypeProvider(new[] {testFixtureType.Assembly}, new[] {testFixtureType.Namespace});

            _bus = new BusBuilder().Configure()
                                   .WithNames("CorrelatorSweepTest", Environment.MachineName)
                                   .WithGlobalPrefix(Guid.NewGuid().ToString())
                                   .WithTransport(new InProcessTransportConfiguration())
                                   .WithTypesFrom(typeProvider)
                                   .WithDependencyResolver(new DependencyResolver(typeProvider))
                                   .WithDefaultTimeout(RequestTimeout)
                                   .WithMaxDeliveryAttempts(1)
                                   .Build();
            await _bus.Start();

            // 1. Put some requests in flight and hold them there.
            var sw = Stopwatch.StartNew();
            var slowRequests = Enumerable.Range(0, SlowRequestCount)
                                         .Select(_ => Capture(() => _bus.Request(new SlowRequest(), RequestTimeout)))
                                         .ToArray();
            await TimeSpan.FromSeconds(5).WaitUntil(() => SlowRequestHandler.Received >= SlowRequestCount);

            // 2. Push the correlator past its 1,000-request sweep threshold.
            var fastFailures = new List<Exception>();
            foreach (var _ in Enumerable.Range(0, FastRequestCount))
            {
                var failure = await Capture(() => _bus.Request(new FastRequest(), RequestTimeout));
                if (failure != null) fastFailures.Add(failure);
            }
            _fastRequestFailures = fastFailures.ToArray();

            // The sweep runs on a background task; give it time to finish before releasing the slow requests.
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            // 3. Let the in-flight requests complete.
            SlowRequestHandler.Gate.SetResult(true);
            // Bounded wait: before the fix for #317 these requests never complete, not even after their own timeout.
            await Task.WhenAny(Task.WhenAll(slowRequests), Task.Delay(RequestTimeout + TimeSpan.FromSeconds(10)));
            _slowRequestFailures = slowRequests.Select(t => t.IsCompleted ? t.Result : new TimeoutException("Request never completed"))
                                               .Where(e => e != null)
                                               .ToArray();
            _slowRequestElapsed = sw.Elapsed;
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            if (_bus != null) await _bus.Stop();
            _bus?.Dispose();
        }

        [Test]
        public void TheInFlightRequestsShouldAllReceiveTheirResponses()
        {
            _slowRequestFailures.Select(e => $"{e.GetType().Name}: {e.Message}").ShouldBeEmpty();
        }

        [Test]
        public void TheInFlightRequestsShouldNotHaveWaitedForTheirTimeout()
        {
            _slowRequestElapsed.ShouldBeLessThan(RequestTimeout);
        }

        [Test]
        public void NoneOfTheRequestsThatTrippedTheSweepShouldFail()
        {
            _fastRequestFailures.Select(e => $"{e.GetType().Name}: {e.Message}").ShouldBeEmpty();
        }

        private static async Task<Exception> Capture(Func<Task> action)
        {
            try
            {
                await action();
                return null;
            }
            catch (Exception exc)
            {
                return exc;
            }
        }
    }
}
