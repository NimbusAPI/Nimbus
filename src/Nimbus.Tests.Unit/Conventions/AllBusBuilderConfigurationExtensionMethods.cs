using System.Linq;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllBusBuilderConfigurationExtensionMethods
    {
        private readonly string _configurationExtensionsNamespace = typeof(BusBuilderConfigurationExtensions).Namespace;

        [Test]
        public void MustAdhereToConventions()
        {
            var assemblies = new[]
                             {
                                 typeof(BusBuilderConfigurationExtensions).Assembly,
                                 typeof(AutofacBusBuilderConfigurationExtensions).Assembly
                             };

            var testCases = assemblies
                            .SelectMany(a => a.DefinedTypes)
                            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                            .Where(m => m.IsExtensionMethodFor<INimbusConfiguration>());

            foreach (var method in testCases)
            {
                method.IsPublic.ShouldBe(true);

                // ReSharper disable once PossibleNullReferenceException
                method.DeclaringType.Namespace.ShouldBe(_configurationExtensionsNamespace);
            }
        }
    }
}