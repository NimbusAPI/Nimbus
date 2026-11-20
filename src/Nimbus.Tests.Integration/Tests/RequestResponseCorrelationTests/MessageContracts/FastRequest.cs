using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.RequestResponseCorrelationTests.MessageContracts
{
    public class FastRequest : IBusRequest<FastRequest, FastResponse>
    {
    }

    public class FastResponse : IBusResponse
    {
    }
}
