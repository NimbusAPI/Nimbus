using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.MulticastRequestResponseTests.MessageContracts
{
    public class SomeRequestThatIsNotReturedByTheTypeProvider : IBusRequest<SomeRequestThatIsNotReturedByTheTypeProvider, SomeResponse>
    {
    }
}