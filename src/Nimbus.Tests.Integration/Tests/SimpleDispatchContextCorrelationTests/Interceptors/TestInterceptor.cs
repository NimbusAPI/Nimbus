using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.Interceptors
{
    public class TestInterceptor : InboundInterceptor, IRequireDispatchContext, IRequireNimbusMessage
    {
        internal static readonly List<IDispatchContext> DispatchContexts = new List<IDispatchContext>();
        internal static readonly List<NimbusMessage> NimbusMessages = new List<NimbusMessage>();

        public IDispatchContext DispatchContext { get; set; }
        public NimbusMessage NimbusMessage { get; set; }

        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            DispatchContexts.Add(DispatchContext);
            NimbusMessages.Add(NimbusMessage);
        }

        public static void Clear()
        {
            DispatchContexts.Clear();
            NimbusMessages.Clear();
        }
    }
}