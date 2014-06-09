using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.MessageContracts
{
    public class ExceptingRequest : IBusRequest<ExceptingRequest, ExceptingResponse>
    {
    }
}