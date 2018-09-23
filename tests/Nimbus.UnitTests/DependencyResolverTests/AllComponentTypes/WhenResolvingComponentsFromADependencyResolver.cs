using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Stubs;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes
{
    [TestFixture]
    public class WhenResolvingComponentsFromADependencyResolver
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void TheComponentShouldResolveCorrectly(IDependencyResolver dependencyResolver, Type componentType)
        {
            using (var scope = dependencyResolver.CreateChildScope())
            {
                var handler = scope.Resolve(componentType);
                //TODO: Fix
                //handler.ShouldBeTypeOf(componentType);
            }
        }

        public class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return Generate().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private IEnumerable<TestCaseData> Generate()
            {
                var typeProvider = new TestHarnessTypeProvider(new[] {Assembly.GetExecutingAssembly()}, new[] {GetType().Namespace});

                var dependencyResolvers = TestHarnessDependencyResolversFactory.GetAllDependencyResolvers(typeProvider);

                var typesToResolve = typeProvider.AllResolvableTypes();

                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var dependencyResolver in dependencyResolvers)
                {
                    foreach (var componentType in typesToResolve)
                    {
                        yield return new TestCaseData(dependencyResolver, componentType)
                            .SetName(dependencyResolver.GetType().Name + "." + componentType.Name);
                    }
                }
                // ReSharper restore LoopCanBeConvertedToQuery
            }
        }
    }
}