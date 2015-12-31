namespace Nimbus.Transports.InProcess.QueueManagement
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