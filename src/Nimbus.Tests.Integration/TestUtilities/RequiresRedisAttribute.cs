using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Nimbus.Tests.Integration.TestUtilities
{
    /// <summary>
    ///     Marks a test as needing a reachable Redis server, independently of which transport the suite
    ///     is running against. Unlike filtering the scenario matrix, this reports as Skipped with a
    ///     reason, so a test that stops running stays visible in the run summary.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresRedisAttribute : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable) return;
            if (RedisAvailability.IsReachable) return;

            test.RunState = RunState.Ignored;
            test.Properties.Set(PropertyNames.SkipReason,
                                $"Redis is not reachable at '{RedisAvailability.ConnectionString}'. Start it with `docker-compose up -d redis`.");
        }
    }
}
