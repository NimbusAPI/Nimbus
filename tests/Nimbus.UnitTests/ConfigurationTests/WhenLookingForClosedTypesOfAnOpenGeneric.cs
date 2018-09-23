using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.ConfigurationTests
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
                yield return new TestCaseData(typeof (ApplicationNameSetting), typeof (Setting<>), true);
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
            public async Task Handle(MyCommand busCommand)
            {
                throw new NotImplementedException();
            }
        }

        public class MyCommand : IBusCommand
        {
        }
    }
}