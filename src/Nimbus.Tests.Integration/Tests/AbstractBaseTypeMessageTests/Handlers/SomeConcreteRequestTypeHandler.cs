using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteRequestTypeHandler : IHandleRequest<SomeConcreteRequestType, SomeConcreteResponseType>, IRequireBusId
    {
        public async Task<SomeConcreteResponseType> Handle(SomeConcreteRequestType request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeConcreteRequestTypeHandler>(ch => ch.Handle(request));

            return new SomeConcreteResponseType();
        }

        public Guid BusId { get; set; }
    }
}