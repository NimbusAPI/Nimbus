using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors;
using Nimbus.IntegrationTests.Tests.InterceptorTests.MessageContracts;
using Nimbus.Interceptors.Inbound;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Handlers
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