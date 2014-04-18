using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.DisposableComponents.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.DisposableComponents.MessageContracts;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DependencyResolverTests.DisposableComponents
{
    public class WhenResolvingADisposableHandler : TestForAllDependencyResolvers
    {
        protected override async Task When()
        {
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheHandlerShouldBeDisposedAfterTheScopeIsDisposed(AllDependencyResolversTestContext context)
        {
            await Given(context);
            await When();

            DisposableHandler handler;
            using (var scope = Subject.CreateChildScope())
            {
                var componentName = typeof (DisposableHandler).FullName;
                handler = (DisposableHandler) scope.Resolve<IHandleCommand<NullCommand>>(componentName);
            }

            handler.IsDisposed.ShouldBe(true);
        }
    }
}