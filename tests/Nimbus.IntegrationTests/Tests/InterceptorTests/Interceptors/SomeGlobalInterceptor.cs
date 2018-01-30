using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public class SomeGlobalInterceptor : InboundInterceptor, IRequireBusId
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeGlobalInterceptor>(h => h.OnCommandHandlerExecuting(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeGlobalInterceptor>(h => h.OnCommandHandlerSuccess(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeGlobalInterceptor>(h => h.OnCommandHandlerError(busCommand, nimbusMessage, exception));
        }

        public Guid BusId { get; set; }
    }
}