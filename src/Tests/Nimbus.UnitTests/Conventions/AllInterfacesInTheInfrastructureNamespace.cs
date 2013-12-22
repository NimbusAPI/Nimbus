using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class AllInterfacesInTheInfrastructureNamespace
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustBeInternal(Type interfaceType)
        {
            interfaceType.IsPublic.ShouldBe(false);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var referenceType = typeof (IMessagePump);

                return referenceType.Assembly.GetTypes()
                                    .Where(t => t.Namespace == referenceType.Namespace || t.Namespace.StartsWith(referenceType.Namespace + "."))
                                    .Where(t => t.IsInterface)
                                    .Select(t => new TestCaseData(t)
                                                     .SetName(t.FullName)
                    ).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}