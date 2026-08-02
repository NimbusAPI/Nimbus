using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Extensions.Pulse.Redis;
using Nimbus.Infrastructure.Logging;
using Nimbus.Tests.Integration.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.PulseTests
{
    /// <summary>
    ///     The coordinator is what stops a Pulse schedule firing once per application instance, so these
    ///     exercise the claim directly against a real Redis rather than through a bus. It is deliberately
    ///     transport-agnostic, so it needs Redis to be reachable but doesn't care which transport the
    ///     rest of the suite is running against.
    /// </summary>
    [TestFixture]
    [RequiresRedis]
    public class WhenClaimingPulseOccurrencesWithRedis
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private string _applicationName;

        [SetUp]
        public void SetUp()
        {
            // A per-test application name keeps runs from colliding with each other, and with anything
            // left behind by an earlier run.
            _applicationName = $"PulseTests-{TestContext.CurrentContext.Test.Name}-{Guid.NewGuid():N}";
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var disposable in _disposables) disposable.Dispose();
            _disposables.Clear();
        }

        private RedisPulseCoordinator CreateCoordinator(TimeSpan? claimTimeToLive = null)
        {
            var coordinator = new RedisPulseCoordinator(RedisAvailability.ConnectionString,
                                                        _applicationName,
                                                        claimTimeToLive ?? TimeSpan.FromMinutes(5),
                                                        new NullLogger());
            _disposables.Add(coordinator);
            return coordinator;
        }

        [Test]
        public async Task OnlyOneOfManyCompetingInstancesShouldWinAnOccurrence()
        {
            const int instanceCount = 20;
            var occurrence = DateTimeOffset.UtcNow;
            var coordinators = Enumerable.Range(0, instanceCount).Select(_ => CreateCoordinator()).ToArray();

            // Hold every instance at the gate so they genuinely contend rather than going one at a time.
            var gate = new TaskCompletionSource();
            var claims = coordinators.Select(async c =>
                                             {
                                                 await gate.Task;
                                                 return await c.TryClaim("some-schedule", occurrence, CancellationToken.None);
                                             })
                                     .ToArray();
            gate.SetResult();

            var results = await Task.WhenAll(claims);

            results.Count(won => won).ShouldBe(1);
        }

        [Test]
        public async Task TheSameOccurrenceShouldNotBeClaimableTwice()
        {
            var coordinator = CreateCoordinator();
            var occurrence = DateTimeOffset.UtcNow;

            (await coordinator.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(true);
            (await coordinator.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(false);
        }

        [Test]
        public async Task ALaterOccurrenceOfTheSameScheduleShouldBeClaimable()
        {
            var coordinator = CreateCoordinator();
            var occurrence = DateTimeOffset.UtcNow;

            (await coordinator.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(true);
            (await coordinator.TryClaim("some-schedule", occurrence.AddMinutes(1), CancellationToken.None)).ShouldBe(true);
        }

        [Test]
        public async Task DifferentSchedulesShouldNotContend()
        {
            var coordinator = CreateCoordinator();
            var occurrence = DateTimeOffset.UtcNow;

            (await coordinator.TryClaim("schedule-one", occurrence, CancellationToken.None)).ShouldBe(true);
            (await coordinator.TryClaim("schedule-two", occurrence, CancellationToken.None)).ShouldBe(true);
        }

        [Test]
        public async Task TheSameOccurrenceInADifferentTimeZoneShouldBeTheSameClaim()
        {
            var coordinator = CreateCoordinator();
            var utc = new DateTimeOffset(2026, 8, 2, 3, 0, 0, TimeSpan.Zero);
            var sameInstantInAdelaide = utc.ToOffset(TimeSpan.FromHours(9.5));

            (await coordinator.TryClaim("some-schedule", utc, CancellationToken.None)).ShouldBe(true);
            (await coordinator.TryClaim("some-schedule", sameInstantInAdelaide, CancellationToken.None)).ShouldBe(false);
        }

        [Test]
        public async Task DifferentApplicationsShouldNotContend()
        {
            var occurrence = DateTimeOffset.UtcNow;
            var sharedSuffix = Guid.NewGuid().ToString("N");

            var appA = new RedisPulseCoordinator(RedisAvailability.ConnectionString, "AppA-" + sharedSuffix, TimeSpan.FromMinutes(5), new NullLogger());
            var appB = new RedisPulseCoordinator(RedisAvailability.ConnectionString, "AppB-" + sharedSuffix, TimeSpan.FromMinutes(5), new NullLogger());
            _disposables.Add(appA);
            _disposables.Add(appB);

            (await appA.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(true);
            (await appB.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(true);
        }

        [Test]
        public async Task AClaimShouldBeReleasedOnceItsTimeToLiveExpires()
        {
            var coordinator = CreateCoordinator(TimeSpan.FromSeconds(1));
            var occurrence = DateTimeOffset.UtcNow;

            (await coordinator.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(true);
            await Task.Delay(TimeSpan.FromSeconds(2));

            // An instance arriving after the claim expires will fire the occurrence again, which is why
            // the time-to-live has to comfortably exceed the clock skew between instances.
            (await coordinator.TryClaim("some-schedule", occurrence, CancellationToken.None)).ShouldBe(true);
        }

        /// <summary>
        ///     Pulse treats a throw as "don't fire", so an outage costs occurrences rather than firing
        ///     them once per instance. Silently returning true here would be the worst of both worlds.
        /// </summary>
        [Test]
        public void AnUnreachableRedisShouldThrowRatherThanGrantTheClaim()
        {
            var coordinator = new RedisPulseCoordinator("localhost:6399,connectTimeout=500,connectRetry=1,syncTimeout=500",
                                                        _applicationName,
                                                        TimeSpan.FromMinutes(5),
                                                        new NullLogger());
            _disposables.Add(coordinator);

            Should.Throw<Exception>(async () => await coordinator.TryClaim("some-schedule", DateTimeOffset.UtcNow, CancellationToken.None));
        }

        [Test]
        public void AClaimTimeToLiveOfZeroShouldBeRejected()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new RedisPulseCoordinator(RedisAvailability.ConnectionString,
                                                                                      _applicationName,
                                                                                      TimeSpan.Zero,
                                                                                      new NullLogger()));
        }
    }
}
