﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.Extensions;
using Shouldly;

namespace Nimbus.Tests.TaskExtensionTests
{
    public class GivenThreeTasksOfVaryingDuration
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
                    Task.Run(() =>
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(200));
                        return 200;
                    }),
                    Task.Run(() =>
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(50));
                        return 20;
                    }),
                    Task.Run(() =>
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(10));
                        return 10;
                    }),
                };
            }

            public override void When()
            {
                _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(100)).ToArray();
                _sw.Stop();
            }

            [Test]
            public void ThereShouldBeTwoResults()
            {
                _result.Count().ShouldBe(2);
            }

            [Test]
            public void OneResultShouldBe10()
            {
                _result.ShouldContain(10);
            }

            [Test]
            public void OneResultShouldBe20()
            {
                _result.ShouldContain(20);
            }

            [Test]
            public void TheElapsedTimeShouldBeLessThanTheSlowestTasksDuration()
            {
                _sw.ElapsedMilliseconds.ShouldBeLessThan(200);
            }
        }
    }
}