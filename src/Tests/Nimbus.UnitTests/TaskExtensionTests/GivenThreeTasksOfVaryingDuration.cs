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
            public void WhenOneTaskReturnsInTime_WeGetOneResult()
            {
                _semaphore1.Release();
                _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(1000)).ToArray();
                
                _result[0].ShouldBe(1);
            }

            [Test]
            public void WhenTwoTaskReturnsInTime_WeGetTwoResults()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(1000)).ToArray();

                _result.Count().ShouldBe(2);
            }

            [Then]
            public void WhenTheThirdResultDoesntComeInTime_WeGetTwoResults()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(1000)).ToArray();

                _stopwatch.Stop();

                _stopwatch.Elapsed.TotalMilliseconds.ShouldBeGreaterThan(1000);
                _result.Count().ShouldBe(2);
            }

        }
    }
}