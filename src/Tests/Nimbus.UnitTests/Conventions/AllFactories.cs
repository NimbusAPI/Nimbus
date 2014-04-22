using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Interceptors;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class AllFactories
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustImplementICreateComponents(Type factoryType)
        {
            typeof (ICreateComponents).IsAssignableFrom(factoryType).ShouldBe(true);
        }

        internal class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return typeof (Bus).Assembly
                                   .GetTypes()
                                   .Where(t => t.Name.EndsWith("Factory"))
                                   .Where(t => GetExcludedTypes().Contains(t) == false)
                                   .Where(t => t.IsInstantiable())
                                   .Where(t => t.GetCustomAttribute<ObsoleteAttribute>() == null)
                                   .Where(t => !t.IsAssignableFrom(typeof (IDependencyResolver)))
                                   .Select(t => new TestCaseData(t)
                                               .SetName(t.FullName))
                                   .GetEnumerator();
            }

            private IEnumerable<Type> GetExcludedTypes()
            {
                yield return typeof (BrokeredMessageFactory);
                yield return typeof (InterceptorFactory);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}