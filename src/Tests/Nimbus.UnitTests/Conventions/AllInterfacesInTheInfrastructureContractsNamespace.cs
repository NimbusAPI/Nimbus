using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.InfrastructureContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Conventions
{
    [TestFixture]
    public class AllInterfacesInTheInfrastructureContractsNamespace
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustBePublic(Type interfaceType)
        {
            interfaceType.IsPublic.ShouldBe(true);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var referenceType = typeof (ICommandBroker);

                return referenceType.Assembly.GetTypes()
                                    .Where(t => t.Namespace.StartsWith(referenceType.Namespace))
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