using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class CoreInfrastructureInterfaces
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustBeInTheCoreNimbusNamespace(Type type)
        {
            type.Namespace.ShouldBe("Nimbus");
        }

        internal class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var coreInfrastructureInterfaces = new[]
                                                   {
                                                       typeof (IBus)
                                                   };

                return coreInfrastructureInterfaces
                    .Select(t => new TestCaseData(t).SetName(t.FullName))
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}