namespace Nimbus
{
    public interface IDispatchContext
    {
        string DispatchId { get; }
        string ResultOfMessageId { get; }
        string CorrelationId { get; }
    }
}