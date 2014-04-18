using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.MulticastEventBrokerTests.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.MulticastEventBrokerTests.MessageContracts;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DependencyResolverTests.MulticastEventBrokerTests
{
    [TestFixture]
    public class WhenResolvingHandlersForAnEventWithTwoHandlers : TestForAllDependencyResolvers
    {
        protected override async Task When()
        {
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ResolvingTheFirstFooHandlerViaNameShouldGiveTheCorrectType(AllDependencyResolversTestContext context)
        {
            await Given(context);
            await When();

            using (var scope = Subject.CreateChildScope())
            {
                var componentName = typeof (FirstFooEventHandler).FullName;
                var handler = scope.Resolve<IHandleMulticastEvent<FooEvent>>(componentName);
                handler.ShouldBeTypeOf<FirstFooEventHandler>();
            }
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ResolvingTheSecondFooHandlerViaNameShouldGiveTheCorrectType(AllDependencyResolversTestContext context)
        {
            await Given(context);
            await When();

            using (var scope = Subject.CreateChildScope())
            {
                var componentName = typeof (SecondFooEventHandler).FullName;
                var handler = scope.Resolve<IHandleMulticastEvent<FooEvent>>(componentName);
                handler.ShouldBeTypeOf<SecondFooEventHandler>();
            }
        }
    }
}