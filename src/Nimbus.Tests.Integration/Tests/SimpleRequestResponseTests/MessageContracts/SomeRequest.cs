using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.SimpleRequestResponseTests.MessageContracts
{
    public class SomeRequest : IBusRequest<SomeRequest, SomeResponse>
    {
    }
}