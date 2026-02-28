using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Conventional;
using Nimbus.Configuration.Settings;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllParametersOnInfrastructureTypes
    {
        [Test]
        public void MustAdhereToConventions()
        {
            typeof(Bus).Assembly
                       .DefinedTypes
                       .Where(t => !IsIgnored(t))
                       .Where(IsInfrastructureType)
                       .MustConformTo(new ParameterOrderConventionSpecification(SortParameters))
                       .WithFailureAssertion(message => throw new Exception(message));
        }

        private static IEnumerable<ParameterInfo> SortParameters(IEnumerable<ParameterInfo> constructorParameters)
        {
            return constructorParameters
                   .OrderByDescending(p => p.ParameterType.IsClosedTypeOf(typeof(Setting<>)))
                   .ThenBy(p => !p.ParameterType.IsInterface)
                   .ThenBy(p => p.ParameterType.Name)
                   .ThenBy(p => p.Name)
                   .ToArray();
        }

        private static bool IsInfrastructureType(Type type)
        {
            return type.GetConstructors()
                       .Where(c => c.GetParameters().Any(p => p.ParameterType.Namespace?.StartsWith("Nimbus") ?? false))
                       .Where(c => c.GetParameters().All(p => (p.ParameterType.Namespace ?? string.Empty).StartsWith("Nimbus")))
                       .Any();
        }

        private static bool IsIgnored(Type declaringType)
        {
            var ignoredTypes = new[] {typeof(Bus)};
            return ignoredTypes.Any(t => t.IsAssignableFrom(declaringType));
        }
    }
}