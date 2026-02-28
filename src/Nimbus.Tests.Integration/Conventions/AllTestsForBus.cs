using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllTestsForBus
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var types = GetType().Assembly
                                 .DefinedTypes
                                 .Where(t => t.IsAssignableTo<TestForBus>())
                                 .Where(t => t.IsInstantiable())
                                 .ToArray();

            foreach (var type in types)
            {
                ShouldHaveASingleTestNamedRun(type);
                ShouldHaveAtLeastOneMethodWithAThenAttribute(type);
            }
        }

        private static void ShouldHaveASingleTestNamedRun(Type fixtureType)
        {
            var testMethods = fixtureType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         .Where(m => m.HasAttribute<TestAttribute>())
                                         .ToArray();

            testMethods.Single().Name.ShouldBe("Run");
        }

        private static void ShouldHaveAtLeastOneMethodWithAThenAttribute(Type fixtureType)
        {
            var thenMethods = fixtureType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         .Where(m => m.HasAttribute<ThenAttribute>())
                                         .ToArray();

            thenMethods.Length.ShouldBeGreaterThan(0);
        }
    }
}