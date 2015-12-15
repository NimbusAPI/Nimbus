namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class Queue
    {
        private readonly string _queuePath;

        public Queue(string queuePath)
        {
            _queuePath = queuePath;
        }

        public string QueuePath
        {
            get { return _queuePath; }
        }
    }
}