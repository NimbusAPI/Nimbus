using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Handlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class CoreHandlerInterfaces
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustBeInTheCorrectNamespaceNamespace(Type type)
        {
            type.Namespace.ShouldBe("Nimbus.Handlers");
        }

        internal class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var coreInfrastructureInterfaces = new[]
                                                   {
                                                       typeof (IHandleCommand<>),
                                                       typeof (IHandleRequest<,>),
                                                       typeof (IHandleMulticastEvent<>),
                                                       typeof (IHandleCompetingEvent<>)
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