using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.RequestResponseHandlerResolutionTests.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.RequestResponseHandlerResolutionTests.MessageContracts;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DependencyResolverTests.RequestResponseHandlerResolutionTests
{
    [TestFixture]
    public class WhenResolvingAHandlersForASimpleRequest : TestForAllDependencyResolvers
    {
        protected override async Task When()
        {
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeAFooRequestHandler(AllDependencyResolversTestContext context)
        {
            await Given(context);
            await When();

            using (var scope = Subject.CreateChildScope())
            {
                var componentName = typeof (FooRequestHandler).FullName;
                var handler = scope.Resolve<IHandleRequest<FooRequest, FooResponse>>(componentName);
                handler.ShouldBeTypeOf<FooRequestHandler>();
            }
        }
    }
}