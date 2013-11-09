namespace Nimbus.Infrastructure
{
    internal interface IMessagePump
    {
        void Start();
        void Stop();
    }
}