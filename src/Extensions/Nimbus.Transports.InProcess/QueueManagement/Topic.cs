using System.Collections.Generic;

namespace Nimbus.Transports.InProcess.QueueManagement
{
    internal class Topic
    {
        private readonly List<string> _subscriptionNames = new List<string>();

        public string TopicPath { get; }

        public Topic(string topicPath)
        {
            TopicPath = topicPath;
        }

        public void Subscribe(string subscriptionName)
        {
            lock (_subscriptionNames)
            {
                if (_subscriptionNames.Contains(subscriptionName)) return;

                _subscriptionNames.Add(subscriptionName);
            }
        }

        public string[] SubscriptionNames
        {
            get
            {
                lock (_subscriptionNames)
                {
                    return _subscriptionNames.ToArray();
                }
            }
        }
    }
}