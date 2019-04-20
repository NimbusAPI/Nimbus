using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests.MessageContracts
{
    public class RequestThatWillThrow : BusRequest<RequestThatWillThrow, RequestThatWillThrowResponse>
    {
    }
}