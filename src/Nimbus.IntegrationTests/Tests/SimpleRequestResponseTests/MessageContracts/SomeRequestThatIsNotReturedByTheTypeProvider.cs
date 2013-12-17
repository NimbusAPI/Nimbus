using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts
{
    public class SomeRequestThatIsNotReturedByTheTypeProvider : IBusRequest<SomeRequestThatIsNotReturedByTheTypeProvider, SomeResponse>
    {
    }
}