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
        public class WhenOpportunisticallyReturningTheirResultsAsTheyComplete : SpecificationFor<OpportunisticTaskCompletionReturner<int>>
        {
            private int[] _result;
            private Semaphore _semaphore0;
            private Semaphore _semaphore1;
            private Semaphore _semaphore2;
            private Task<int>[] _tasks;
            private CancellationTokenSource _cancellationToken;

            public override OpportunisticTaskCompletionReturner<int> Given()
            {
                _semaphore0 = new Semaphore(0, 1);
                _semaphore1 = new Semaphore(0, 1);
                _semaphore2 = new Semaphore(0, 1);

                _tasks = new[]
                         {
                             Task.Run(() =>
                                      {
                                          _semaphore0.WaitOne();
                                          return 0;
                                      }),
                             Task.Run(() =>
                                      {
                                          _semaphore1.WaitOne();
                                          return 1;
                                      }),
                             Task.Run(() =>
                                      {
                                          _semaphore2.WaitOne();
                                          return 2;
                                      })
                         };

                _cancellationToken = new CancellationTokenSource();
                return new OpportunisticTaskCompletionReturner<int>(_tasks, _cancellationToken);
            }

            public override void When()
            {
            }

            public override void TearDown()
            {
                _semaphore0.Dispose();
                _semaphore1.Dispose();
                _semaphore2.Dispose();
                _cancellationToken.Dispose();

                base.TearDown();
            }

            [Test]
            public void WhenTakingOnlyTheFirstResult_TheCallShouldExitWithoutNeedingTheCancellationTokenSet()
            {
                _semaphore0.Release();
                _result = Subject.GetResults().Take(1).ToArray();

                _result.Length.ShouldBe(1);

                _cancellationToken.Cancel();
            }

            [Test]
            public async Task WhenOnlyOneTaskReturnsInTime_WeGetASingleCorrectResult()
            {
                await Task.Run(() =>
                               {
                                   _semaphore0.Release();
                                   await _tasks[0];
                                   _cancellationToken.Cancel();

                                   _result = Subject.GetResults().ToArray();

                                   _result.Length.ShouldBe(1);
                                   _result[0].ShouldBe(0);
                                   _cancellationToken.IsCancellationRequested.ShouldBe(true);
                               });
            }

            [Test]
            public async Task WhenTwoTasksReturnInTime_WeGetTwoCorrectResults()
            {
                await Task.Run(async () =>
                                     {
                                         _semaphore0.Release();
                                         _semaphore1.Release();

                                         await Subject.Continuations[0];
                                         await Subject.Continuations[1];

                                         _cancellationToken.Cancel();

                                         _semaphore2.Release();
                                         await Subject.Continuations[2];

                                         _result = Subject.GetResults().ToArray();

                                         _result.Count().ShouldBe(2);
                                         _result.ShouldContain(0);
                                         _result.ShouldContain(1);
                                     });
            }

            [Test]
            public void WhenAllTasksCompleteImmediately_WeShouldGetThreeResults()
            {
                _semaphore0.Release();
                _semaphore1.Release();
                _semaphore2.Release();

                _result = Subject.GetResults().ToArray();

                _result.Count().ShouldBe(3);
            }

            [Test]
            public void WhenAllTasksCompleteImmediately_TheCallShouldExitWithoutTheCancellationTokenBeingSet()
            {
                _semaphore0.Release();
                _semaphore1.Release();
                _semaphore2.Release();

                _result = Subject.GetResults().ToArray();

                _cancellationToken.IsCancellationRequested.ShouldBe(false);
            }
        }
    }
}