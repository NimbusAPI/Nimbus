using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.TaskExtensionTests
{
    public class GivenThreeCompletedTasks
    {
        [TestFixture]
        public class WhenOpportunisticallyReturningTheirResultsAsTheyComplete : SpecificationFor<Task<int>[]>
        {
            private int[] _result;

            protected override Task<int>[] Given()
            {
                return new[]
                       {
                           Task.FromResult(1),
                           Task.FromResult(2),
                           Task.FromResult(3)
                       };
            }

            protected override void When()
            {
                _result = Subject.ReturnOpportunistically(TimeSpan.FromMilliseconds(100)).ToArray();
            }

            [Test]
            public void ThereShouldBeThreeResults()
            {
                _result.Count().ShouldBe(3);
            }
        }
    }
}