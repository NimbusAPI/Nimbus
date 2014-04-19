namespace Nimbus.Handlers
{
    public interface ILongRunningTask
    {
        bool IsAlive { get; }
    }
}