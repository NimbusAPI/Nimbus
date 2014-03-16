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
    public class GivenThreeTasksOfVaryingDuration
    {
        [TestFixture]
        public class WhenOpportunisticallyReturningTheirResultsAsTheyComplete : SpecificationFor<Task<int>[]>
        {
            private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(1000);

            private int[] _result;
            private Semaphore _semaphore1;
            private Semaphore _semaphore2;
            private Semaphore _semaphore3;
            private Stopwatch _stopwatch;

            public override Task<int>[] Given()
            {
                _stopwatch = Stopwatch.StartNew();

                _semaphore1 = new Semaphore(0, 1);
                _semaphore2 = new Semaphore(0, 1);
                _semaphore3 = new Semaphore(0, 1);

                return new[]
                       {
                           Task.Run(() =>
                                    {
                                        _semaphore1.WaitOne();
                                        return 1;
                                    }),
                           Task.Run(() =>
                                    {
                                        _semaphore2.WaitOne();
                                        return 2;
                                    }),
                           Task.Run(() =>
                                    {
                                        _semaphore3.WaitOne();
                                        return 3;
                                    })
                       };
            }

            public override void When()
            {
            }

            [Test]
            public void WhenTakingOnlyTheFirstResult_TheElapsedTimeShouldBeLessThanTheTimeout()
            {
                _semaphore1.Release();
                _result = Subject.ReturnOpportunistically(_timeout).Take(1).ToArray();
                _stopwatch.Stop();

                _stopwatch.Elapsed.ShouldBeLessThan(_timeout);
            }

            [Test]
            public void WhenOnlyOneTaskReturnsInTime_WeGetASingleCorrectResult()
            {
                _semaphore1.Release();
                _result = Subject.ReturnOpportunistically(_timeout).ToArray();

                _result.Length.ShouldBe(1);
                _result[0].ShouldBe(1);
            }

            [Test]
            public void WhenOnlyOneTaskReturnsInTime_TheElapsedTimeShouldBeGreaterThanTheTimeout()
            {
                _semaphore1.Release();
                _result = Subject.ReturnOpportunistically(_timeout).ToArray();

                _stopwatch.Stop();

                _stopwatch.Elapsed.ShouldBeGreaterThan(_timeout);
            }

            [Test]
            public void WhenTwoTaskReturnsInTime_WeGetTwoCorrectResults()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _result = Subject.ReturnOpportunistically(_timeout).ToArray();

                _result.Count().ShouldBe(2);
                _result.ShouldContain(1);
                _result.ShouldContain(2);
            }

            [Test]
            public void WhenAllTasksCompleteImmediately_WeShouldGetThreeResults()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _semaphore3.Release();
                _result = Subject.ReturnOpportunistically(_timeout).ToArray();
                _stopwatch.Stop();

                _result.Count().ShouldBe(3);
            }

            [Test]
            public void WhenAllTasksCompleteImmediately_TheElapsedTimeShouldBeLessThanTheTimeout()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _semaphore3.Release();
                _result = Subject.ReturnOpportunistically(_timeout).ToArray();
                _stopwatch.Stop();

                _stopwatch.Elapsed.ShouldBeLessThan(_timeout);
            }
        }
    }
}