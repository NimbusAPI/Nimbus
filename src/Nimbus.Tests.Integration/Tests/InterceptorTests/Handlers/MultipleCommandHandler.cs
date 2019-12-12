using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.InterceptorTests.Interceptors;
using Nimbus.Tests.Integration.Tests.InterceptorTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.InterceptorTests.Handlers
{
    [Interceptor(typeof (SomeClassLevelInterceptor))]
    public class MultipleCommandHandler : SomeBaseCommandHandler, IRequireBusId
    {
        [Interceptor(typeof (SomeMethodLevelInterceptorForFoo))]
        public override async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<MultipleCommandHandler>(h => h.Handle(busCommand));
        }

        [Interceptor(typeof (SomeMethodLevelInterceptorForBar))]
        public override async Task Handle(BarCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<MultipleCommandHandler>(h => h.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}