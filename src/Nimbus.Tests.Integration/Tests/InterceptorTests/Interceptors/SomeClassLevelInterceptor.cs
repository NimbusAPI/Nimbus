using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.Tests.Integration.Tests.InterceptorTests.Interceptors
{
    public class SomeClassLevelInterceptor : InboundInterceptor, IRequireBusId
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeClassLevelInterceptor>(h => h.OnCommandHandlerExecuting(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeClassLevelInterceptor>(h => h.OnCommandHandlerSuccess(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeClassLevelInterceptor>(h => h.OnCommandHandlerError(busCommand, nimbusMessage, exception));
        }

        public Guid BusId { get; set; }
    }
}