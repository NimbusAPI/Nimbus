using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.MessageContracts
{
    public class SlowRequest : IBusRequest<SlowRequest, SlowResponse>
    {
    }

    public class SlowResponse : IBusResponse
    {
    }
}
