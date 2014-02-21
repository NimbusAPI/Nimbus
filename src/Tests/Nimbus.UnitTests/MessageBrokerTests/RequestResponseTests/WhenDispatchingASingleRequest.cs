using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests.MessageContracts;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MessageBrokerTests.RequestResponseTests
{
    [TestFixture]
    public class WhenDispatchingASingleRequest : TestForAll<IRequestHandlerFactory>
    {
        private FooRequest _request;
        private FooResponse _response;

        protected override async Task When()
        {
            _request = new FooRequest();
            Assert.Fail();
            //_response = Subject.Handle<FooRequest, FooResponse>(_request);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheFirstEventHandlerShouldReceiveACopy(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            MethodCallCounter.ReceivedCallsWithAnyArg<FooRequestHandler>(h => h.Handle(_request)).ShouldContain(_request);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task TheResponseShouldBeAFooResponse(AllSubjectsTestContext context)
        {
            await Given(context);
            await When();

            _response.ShouldBeTypeOf<FooResponse>();
        }
    }
}