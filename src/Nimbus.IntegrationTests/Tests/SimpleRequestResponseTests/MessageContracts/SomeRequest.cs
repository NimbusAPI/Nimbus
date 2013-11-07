using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts
{
    public class SomeRequest : BusRequest<SomeRequest, SomeResponse>
    {
    }
}