using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Logging
{
    internal static class DispatchLoggingContext
    {
        internal static NimbusMessage NimbusMessage
        {
            get { return CallContext.LogicalGetData("NimbusMessage") as NimbusMessage; }
            set { CallContext.LogicalSetData("NimbusMessage", value); }
        }
    }
}