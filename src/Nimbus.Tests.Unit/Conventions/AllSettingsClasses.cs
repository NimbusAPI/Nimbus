using System;
using System.Linq;
using Conventional;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllSettingsClasses
    {
        [Test]
        public void MustAdhereToConventions()
        {
            typeof(Setting<>).Assembly
                             .GetTypes()
                             .Where(t => t.IsClosedTypeOf(typeof(Setting<>)))
                             .MustConformTo(new MustBePublicConventionSpecification())
                             .AndMustConformTo(new MustBeInMatchingNamespaceConventionSpecification(ns => ns.Contains(".Configuration.")))
                             .AndMustConformTo(new MustBeInMatchingNamespaceConventionSpecification(ns => ns.EndsWith(".Settings")))
                             .WithFailureAssertion(message => throw new Exception(message));
        }
    }
}