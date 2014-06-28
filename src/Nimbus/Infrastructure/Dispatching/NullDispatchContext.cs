namespace Nimbus.Infrastructure.Dispatching
{
    internal class NullDispatchContext : IDispatchContext
    {
        public string MessageId { get { return null; } }
        public string DispatchId { get { return null; } }
        public string CorrelationId { get { return null; } }
    }
}