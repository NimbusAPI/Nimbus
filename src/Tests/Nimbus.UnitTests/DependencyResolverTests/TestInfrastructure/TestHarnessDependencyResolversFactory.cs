using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Tests.Common;
using Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure
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