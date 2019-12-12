using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.ExceptionPropagationTests.MessageContracts
{
    public class RequestThatWillThrow : BusRequest<RequestThatWillThrow, RequestThatWillThrowResponse>
    {
    }
}