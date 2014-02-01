namespace Nimbus.Infrastructure
{
    public interface IMessagePump
    {
        void Start();
        void Stop();
    }
}