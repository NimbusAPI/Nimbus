using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes
{
    public class SomeMulticastRequest : IBusMulticastRequest<SomeMulticastRequest, SomeMulticastResponse>
    {
    }
}