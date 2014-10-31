namespace Nimbus.Infrastructure.Dispatching
{
    internal class InitialDispatchContext : IDispatchContext
    {
        public string ResultOfMessageId { get { return null; } }
        public string DispatchId { get { return null; } }
        public string CorrelationId { get { return null; } }
    }
}