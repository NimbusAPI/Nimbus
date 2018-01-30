using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers
{
    public class SomeConcreteRequestTypeMulticastHandler : IHandleMulticastRequest<SomeConcreteRequestType, SomeConcreteResponseType>, IRequireBusId
    {
        public async Task<SomeConcreteResponseType> Handle(SomeConcreteRequestType request)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeConcreteRequestTypeMulticastHandler>(ch => ch.Handle(request));

            return new SomeConcreteResponseType();
        }

        public Guid BusId { get; set; }
    }
}