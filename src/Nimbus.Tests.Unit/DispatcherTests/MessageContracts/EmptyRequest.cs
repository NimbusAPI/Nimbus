using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.MessageContracts
{
    public class EmptyRequest : IBusRequest<EmptyRequest, EmptyResponse>
    {
    }
}