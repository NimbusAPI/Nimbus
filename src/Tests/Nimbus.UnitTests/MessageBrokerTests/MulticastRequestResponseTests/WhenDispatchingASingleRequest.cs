using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.MulticastRequestResponseTests
{
    [TestFixture]
    public class WhenDispatchingASingleRequest : TestForAll<IMulticastRequestHandlerFactory>
    {
        private FooRequest _request;
        private FooResponse[] _responses;

        protected override async Task When()
        {
            _request = new FooRequest();
            Assert.Fail();
            //_responses = Subject.HandleMulticast<FooRequest, FooResponse>(_request, TimeSpan.FromMilliseconds(200)).ToArray();
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheFirstEventHandlerShouldReceiveACopy(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<FirstFooRequestHandler>(h => h.Handle(_request)).ShouldContain(_request);
        }

        [TestCaseSource("TestCases")]
        public async Task TheSecondEventHandlerShouldReceiveACopy(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<SecondFooRequestHandler>(h => h.Handle(_request)).ShouldContain(_request);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeTwoResponses(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _responses.Count().ShouldBe(2);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task ThereShouldBeAFooResponseFromEachHandler(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _responses.ShouldContain(r => r.WhereDidIComeFrom == typeof (FirstFooRequestHandler).Name);
            _responses.ShouldContain(r => r.WhereDidIComeFrom == typeof (SecondFooRequestHandler).Name);
        }
    }
}