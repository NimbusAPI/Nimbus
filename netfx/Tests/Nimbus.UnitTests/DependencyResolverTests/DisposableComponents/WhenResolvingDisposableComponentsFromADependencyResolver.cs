using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.DependencyResolution;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Stubs;
using Nimbus.UnitTests.DependencyResolverTests.DisposableComponents.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DependencyResolverTests.DisposableComponents
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