using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.Extensions;
using Shouldly;

namespace Nimbus.Tests.TaskExtensionTests
{
    public class GivenThreeCompletedTasks
    {
        [TestFixture]
        public class WhenOpprtunisticallyReturningTheirResultsAsTheyComplete : SpecificationFor<Task<int>[]>
        {
            private int[] _result;

            public override Task<int>[] Given()
            {
                return new[]
                {
                    Task.FromResult(1),
                    Task.FromResult(2),
                    Task.FromResult(3),
                };
            }

            public override void When()
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