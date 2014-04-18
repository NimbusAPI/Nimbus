using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseHandlerResolutionTests.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseHandlerResolutionTests.MessageContracts;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DependencyResolverTests.MulticastRequestResponseHandlerResolutionTests
{
    [TestFixture]
    public class WhenResolvingHandlersForAnRequestWithTwoHandlers : TestForAllDependencyResolvers
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
                var componentName = typeof (FirstFooRequestHandler).FullName;
                var handler = scope.Resolve<IHandleRequest<FooRequest, FooResponse>>(componentName);
                handler.ShouldBeTypeOf<FirstFooRequestHandler>();
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
                var componentName = typeof (SecondFooRequestHandler).FullName;
                var handler = scope.Resolve<IHandleRequest<FooRequest, FooResponse>>(componentName);
                handler.ShouldBeTypeOf<SecondFooRequestHandler>();
            }
        }
    }
}