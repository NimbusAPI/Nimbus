using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.MessageContracts
{
    public class ExceptingRequest : IBusRequest<ExceptingRequest, ExceptingResponse>
    {
    }
}