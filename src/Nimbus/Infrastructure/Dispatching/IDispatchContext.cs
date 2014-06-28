namespace Nimbus.Infrastructure.Dispatching
{
    internal interface IDispatchContext
    {
        string DispatchId { get; }
        string MessageId { get; }
        string CorrelationId { get; }
    }
}