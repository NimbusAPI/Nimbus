using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions.Pulse.Configuration;
using Nimbus.Extensions.Pulse.Redis.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Integration.Tests.PulseTests.MessageContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Integration.TestUtilities;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.PulseTests
{
    /// <summary>
    ///     Covers the seam between the Pulse engine and the coordinator, which nothing else does: that
    ///     the engine claims an occurrence before firing it, that the name it claims under is the one the
    ///     coordinator keys on, and that the bus lifecycle events start and stop it.
    ///     Each bus uses the InProcess transport, so its handler only sees its own sends. A schedule that
    ///     fired on two instances therefore shows up as one occurrence handled twice.
    /// </summary>
    [TestFixture]
    [RequiresRedis]
    public class WhenSeveralInstancesRunTheSamePulseSchedule
    {
        private const int InstanceCount = 3;
        private const int ScheduleIntervalSeconds = 2;

        private Bus[] _buses;

        [OneTimeSetUp]
        public async Task Given()
        {
            PulseHeartbeatCommandHandler.FiredOccurrences.Clear();

            // A per-run application name scopes the Redis claim keys, so a previous run's claims can't
            // suppress this one's.
            var applicationName = $"PulseE2E-{Guid.NewGuid():N}";
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {typeof(PulseHeartbeatCommand).Namespace});

            _buses = Enumerable.Range(0, InstanceCount)
                               .Select(i => new BusBuilder()
                                            .Configure()
                                            .WithNames(applicationName, "instance-" + i)
                                            .WithTransport(new InProcessTransportConfiguration())
                                            .WithTypesFrom(typeProvider)
                                            .WithDependencyResolver(new DependencyResolver(typeProvider))
                                            .WithLogger(TestHarnessLoggerFactory.Create(Guid.NewGuid(), $"{GetType().FullName}.instance-{i}"))
                                            .WithPulse(p => p.Add($"*/{ScheduleIntervalSeconds} * * * * *", new PulseHeartbeatCommand()))
                                            .WithRedisCoordinator(RedisAvailability.ConnectionString)
                                            .Build())
                               .ToArray();

            foreach (var bus in _buses) await bus.Start();

            // Long enough to cross several occurrences, so a coordinator that only works once would show up.
            await Task.Delay(TimeSpan.FromSeconds(ScheduleIntervalSeconds * 3 + 1));

            foreach (var bus in _buses) await bus.Stop();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            foreach (var bus in _buses ?? Array.Empty<Bus>()) bus.Dispose();
            _buses = null;
        }

        private static DateTimeOffset[] Fired => PulseHeartbeatCommandHandler.FiredOccurrences.ToArray();

        /// <summary>
        ///     The property actually under test. Asserting on a total count would be racy near occurrence
        ///     boundaries; asserting that no occurrence was handled twice is not.
        /// </summary>
        [Test]
        public void NoOccurrenceShouldHaveFiredOnMoreThanOneInstance()
        {
            var duplicates = Fired.GroupBy(occurrence => occurrence)
                                  .Where(g => g.Count() > 1)
                                  .Select(g => $"{g.Key:o} fired {g.Count()} times")
                                  .ToArray();

            duplicates.ShouldBeEmpty();
        }

        [Test]
        public void TheScheduleShouldHaveFiredRepeatedly()
        {
            Fired.Distinct().Count().ShouldBeGreaterThanOrEqualTo(2);
        }

        [Test]
        public void EveryFireShouldCarryTheScheduledOccurrenceRatherThanTheWallClock()
        {
            // */2 in the seconds field means occurrences land on even seconds with no sub-second component.
            foreach (var occurrence in Fired)
            {
                occurrence.Millisecond.ShouldBe(0);
                (occurrence.Second % ScheduleIntervalSeconds).ShouldBe(0);
            }
        }

        [Test]
        public void TheOccurrencesShouldBeSpacedByTheScheduleInterval()
        {
            var distinct = Fired.Distinct().OrderBy(o => o).ToArray();
            if (distinct.Length < 2) Assert.Inconclusive("Not enough occurrences observed to measure spacing.");

            var gaps = new List<TimeSpan>();
            for (var i = 1; i < distinct.Length; i++) gaps.Add(distinct[i] - distinct[i - 1]);

            gaps.ShouldAllBe(gap => gap == TimeSpan.FromSeconds(ScheduleIntervalSeconds));
        }
    }
}
