using System.Linq;
using System.Reflection;
using Nimbus.Tests.Common.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllExtensionMethodsNotOnPublicInterfaces
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var assemblies = new[]
                             {
                                 typeof(Bus).Assembly
                             };

            var extensionMethods = assemblies
                                   .SelectMany(a => a.DefinedTypes)
                                   .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                                   .Where(m => m.IsExtensionMethod());

            foreach (var method in extensionMethods)
            {
                if (method.GetParameters().First().ParameterType.IsPublic) continue;

                method.IsAssembly.ShouldBeTrue($"Method {method.DeclaringType}.{method.Name} must be internal");

                // ReSharper disable once PossibleNullReferenceException
                method.DeclaringType.IsPublic.ShouldBeFalse($"Extension method class {method.DeclaringType} must be internal");
            }
        }
    }
}