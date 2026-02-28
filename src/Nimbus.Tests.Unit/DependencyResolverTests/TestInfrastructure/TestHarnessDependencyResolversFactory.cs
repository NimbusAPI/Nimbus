using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts.DependencyResolution;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes;
using Nimbus.Tests.Unit.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories;

namespace Nimbus.Tests.Unit.DependencyResolverTests.TestInfrastructure
{
    internal static class TestHarnessDependencyResolversFactory
    {
        public static IEnumerable<IDependencyResolver> GetAllDependencyResolvers(TestHarnessTypeProvider typeProvider)
        {
            var dependencyResolvers = AssembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => typeof (IDependencyResolverFactory).IsAssignableFrom(t))
                .Where(t => t.IsInstantiable())
                .Select(Activator.CreateInstance)
                .Cast<IDependencyResolverFactory>()
                .Select(factory => factory.Create(typeProvider))
                .ToArray();

            return dependencyResolvers;
        }

        public static IEnumerable<Assembly> AssembliesToScan
        {
            get { yield return typeof (WhenResolvingComponentsFromADependencyResolver).Assembly; }
        }
    }
}