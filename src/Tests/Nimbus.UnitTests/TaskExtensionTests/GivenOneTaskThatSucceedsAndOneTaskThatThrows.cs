using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.TaskExtensionTests
{
    public class GivenOneTaskThatSucceedsAndOneTaskThatThrows
    {
        [TestFixture]
        public class WhenOpportunisticallyReturningTheirResultsAsTheyComplete
        {
            private const int _timeoutMilliseconds = 1000;

            public async Task<Task<int>[]> Given()
            {
                return new[]
                       {
                           Task.Run(() => GoBang()),
                           Task.Run(() => 20)
                       };
            }

            [DebuggerStepThrough]
            private static int GoBang()
            {
                throw new Exception("This task is supposed to go bang.");
            }

            public async Task<int[]> When(Task<int>[] tasks)
            {
                var timeout = TimeSpan.FromMilliseconds(_timeoutMilliseconds);
                return tasks.ReturnOpportunistically(timeout).ToArray();
            }

            [Test]
            public async Task ThereShouldBeOneResult()
            {
                var subject = await Given();
                var result = await When(subject);
                result.Count().ShouldBe(1);
            }

            [Test]
            public async Task TheFirstResultShouldBe20()
            {
                var subject = await Given();
                var result = await When(subject);
                result[0].ShouldBe(20);
            }

            [Test]
            public async Task TheElapsedTimeShouldBeLessThanTheTimeoutBecauseBothTasksCompleteImmediately()
            {
                var sw = Stopwatch.StartNew();
                var subject = await Given();
                var result = await When(subject);
                sw.Stop();

                sw.Elapsed.TotalMilliseconds.ShouldBeLessThan(_timeoutMilliseconds);
            }
        }
    }
}