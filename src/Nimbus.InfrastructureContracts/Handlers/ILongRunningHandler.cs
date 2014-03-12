namespace Nimbus.Handlers
{
    public interface ILongRunningHandler
    {
        bool IsAlive { get; }
    }
}