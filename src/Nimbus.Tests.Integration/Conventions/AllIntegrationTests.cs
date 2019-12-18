using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllIntegrationTests
    {
        public void MustAdhereToConventions()
        {
            var methods = GetType().Assembly
                                   .GetExportedTypes()
                                   .Where(t => t.GetCustomAttributes<TestFixtureAttribute>().Any())
                                   .SelectMany(t => t.GetMethods())
                                   .Where(m => m.GetCustomAttributes<TestAttribute>().Any())
                                   .ToArray();

            foreach (var method in methods)
            {
                ShouldBeAsync(method);
                ShouldReturnTask(method);
                //ShouldHaveATimeout(method);
            }
        }

        private static void ShouldReturnTask(MethodInfo testMethod)
        {
            typeof(Task).IsAssignableFrom(testMethod.ReturnType).ShouldBe(true);
        }

        private static void ShouldBeAsync(MethodInfo testMethod)
        {
            testMethod.HasAttribute<AsyncStateMachineAttribute>().ShouldBe(true);
        }

        private static void ShouldHaveATimeout(MethodInfo testMethod)
        {
            if (testMethod.HasAttribute<TimeoutAttribute>()) return;

            var fixtureTypeHierarchy = new[] {testMethod.DeclaringType}.DepthFirst(t => t.BaseType != null ? new[] {t.BaseType} : new Type[0]);
            if (fixtureTypeHierarchy.Any(t => t.HasAttribute<TimeoutAttribute>())) return;

            Assert.Fail();
        }
    }
}