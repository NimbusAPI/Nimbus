using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.AuditingInterceptorTests.MessageTypes
{
    public class SomeRequest : IBusRequest<SomeRequest, SomeResponse>
    {
    }
}