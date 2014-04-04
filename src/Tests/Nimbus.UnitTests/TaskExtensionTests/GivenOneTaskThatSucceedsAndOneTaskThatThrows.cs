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
            private const int _timeoutMilliseconds = 10000;

            private int[] _result;

            public override Task<int>[] Given()
            {
                _result = null;

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

            public override void When()
            {
                Task.Run(() => { _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(_timeoutMilliseconds)).ToArray(); }).Wait();
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
                ElapsedTime.TotalMilliseconds.ShouldBeLessThan(_timeoutMilliseconds);
            }
        }
    }
}