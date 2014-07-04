using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteRequestTypeHandler : IHandleRequest<SomeConcreteRequestType, SomeConcreteResponseType>
    {
        public async Task<SomeConcreteResponseType> Handle(SomeConcreteRequestType request)
        {
            MethodCallCounter.RecordCall<SomeConcreteRequestTypeHandler>(ch => ch.Handle(request));

            return new SomeConcreteResponseType();
        }
    }
}