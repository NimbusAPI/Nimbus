using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.InfrastructureContracts.DependencyResolution;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Unit.DependencyResolverTests.DisposableComponents.Handlers;
using Nimbus.Tests.Unit.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.DependencyResolverTests.DisposableComponents
{
    [TestFixture]
    public class WhenResolvingDisposableComponentsFromADependencyResolver
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void DisposableComponentsShouldBeDisposed(IDependencyResolver dependencyResolver)
        {
            DisposableHandler handler;
            using (var scope = dependencyResolver.CreateChildScope())
            {
                handler = scope.Resolve<DisposableHandler>();
            }

            handler.IsDisposed.ShouldBe(true);
        }

        public class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var typeProvider = new TestHarnessTypeProvider(new[] {Assembly.GetExecutingAssembly()}, new[] {GetType().Namespace});
                return TestHarnessDependencyResolversFactory.GetAllDependencyResolvers(typeProvider)
                                                            .Select(dr => new TestCaseData(dr))
                                                            .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}