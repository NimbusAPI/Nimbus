using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.TaskExtensionTests
{
    internal class GivenThreeTasksOfVaryingDuration
    {
        [TestFixture]
        [Timeout(1000)]
        public class WhenOpportunisticallyReturningTheirResultsAsTheyComplete : SpecificationFor<OpportunisticTaskCompletionReturner<int>>
        {
            private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

            private int[] _result;
            private Semaphore _semaphore1;
            private Semaphore _semaphore2;
            private Semaphore _semaphore3;
            private Task<int>[] _tasks;
            private CancellationTokenSource _cancellationToken;

            public override OpportunisticTaskCompletionReturner<int> Given()
            {
                _semaphore1 = new Semaphore(0, 1);
                _semaphore2 = new Semaphore(0, 1);
                _semaphore3 = new Semaphore(0, 1);

                _tasks = new[]
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

                _cancellationToken = new CancellationTokenSource(_timeout);
                return new OpportunisticTaskCompletionReturner<int>(_tasks, _cancellationToken);
            }

            public override void When()
            {
            }

            [Test]
            public void WhenTakingOnlyTheFirstResult_TheCallShouldExitWithoutNeedingTheCancellationTokenSet()
            {
                _semaphore1.Release();
                _result = Subject.GetResults().Take(1).ToArray();

                _cancellationToken.IsCancellationRequested.ShouldBe(false);
            }

            [Test]
            public void WhenOnlyOneTaskReturnsInTime_WeGetASingleCorrectResult()
            {
                _semaphore1.Release();
                _result = Subject.GetResults().ToArray();

                _result.Length.ShouldBe(1);
                _result[0].ShouldBe(1);
            }

            [Test]
            public void WhenTwoTaskReturnsInTime_WeGetTwoCorrectResults()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _result = Subject.GetResults().ToArray();

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
                _result = Subject.GetResults().ToArray();

                _result.Count().ShouldBe(3);
            }

            [Test]
            public void WhenAllTasksCompleteImmediately_TheCallShouldExitWithoutTheCancellationTokenBeingSet()
            {
                _semaphore1.Release();
                _semaphore2.Release();
                _semaphore3.Release();
                _result = Subject.GetResults().ToArray();

                _cancellationToken.IsCancellationRequested.ShouldBe(false);
            }
        }
    }
}