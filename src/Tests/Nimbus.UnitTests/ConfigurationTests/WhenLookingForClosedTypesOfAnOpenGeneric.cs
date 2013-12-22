using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.Tests.ConfigurationTests
{
    [TestFixture]
    public class WhenLookingForClosedTypesOfAnOpenGeneric
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void WeShouldGetTheRightAnswer(Type candidateType, Type openGenericType, bool expectedResult)
        {
            candidateType.IsClosedTypeOf(openGenericType).ShouldBe(expectedResult);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                yield return new TestCaseData(typeof (IHandleCommand<>), typeof (IHandleCommand<>), false);
                yield return new TestCaseData(typeof (IHandleCommand<MyCommand>), typeof (IHandleCommand<>), true);
                yield return new TestCaseData(typeof (MyCommandHandler), typeof (IHandleCommand<>), true);
                yield return new TestCaseData(typeof (MyDerivedCommandHandler), typeof (IHandleCommand<>), true);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class MyDerivedCommandHandler : MyCommandHandler
        {
        }

        public class MyCommandHandler : IHandleCommand<MyCommand>
        {
            public void Handle(MyCommand busCommand)
            {
                throw new NotImplementedException();
            }
        }

        public class MyCommand : IBusCommand
        {
        }
    }
}