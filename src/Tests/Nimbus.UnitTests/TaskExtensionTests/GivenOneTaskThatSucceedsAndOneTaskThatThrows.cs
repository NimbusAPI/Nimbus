using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.TaskExtensionTests
{
    public class GivenOneTaskThatSucceedsAndOneTaskThatThrows
    {
        [TestFixture]
        public class WhenOpportunisticallyReturningTheirResultsAsTheyComplete : SpecificationFor<Task<int>[]>
        {
            private int[] _result;
            private Stopwatch _sw;

            public override Task<int>[] Given()
            {
                _sw = Stopwatch.StartNew();

                return new[]
                {
                    Task.Run(() => GoBang()),
                    Task.FromResult(20),
                };
            }

            [DebuggerStepThrough]
            private static int GoBang()
            {
                throw new Exception("This task is supposed to go bang.");
            }

            public override void When()
            {
                _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(100)).ToArray();
                _sw.Stop();
            }

            [Test]
            public void ThereShouldBeOneResult()
            {
                _result.Count().ShouldBe(1);
            }

            [Test]
            public void TheFirstResultShouldBe20()
            {
                _result[0].ShouldBe(20);
            }

            [Test]
            public void TheElapsedTimeShouldBeLessThanTheTimeoutBecauseBothTasksCompleteImmediately()
            {
                _sw.ElapsedMilliseconds.ShouldBeLessThan(100);
            }
        }
    }
}