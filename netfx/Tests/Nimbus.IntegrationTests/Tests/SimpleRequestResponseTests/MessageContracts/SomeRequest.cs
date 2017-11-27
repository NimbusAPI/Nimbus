using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts
{
    public class SomeRequest : IBusRequest<SomeRequest, SomeResponse>
    {
    }
}