using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.MessageContracts
{
    public class EmptyRequest : IBusRequest<EmptyRequest, EmptyResponse>
    {
    }
}