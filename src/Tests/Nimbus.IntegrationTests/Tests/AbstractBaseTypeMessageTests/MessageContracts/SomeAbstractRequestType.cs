using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts
{
    public abstract class SomeAbstractRequestType<TBusRequest, TBusResponse> : IBusRequest<TBusRequest, TBusResponse>
        where TBusResponse : IBusResponse
        where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
    {
    }
}