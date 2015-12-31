using System.Collections.Generic;

namespace Nimbus.Transports.Redis.QueueManagement
{
    internal class Topic
    {
        private readonly string _topicPath;
        private readonly List<string> _subscriptionNames = new List<string>();

        public Topic(string topicPath)
        {
            _topicPath = topicPath;
        }

        public void Subscribe(string subscriptionName)
        {
            lock (_subscriptionNames)
            {
                if (_subscriptionNames.Contains(subscriptionName)) return;

                _subscriptionNames.Add(subscriptionName);
            }
        }

        public string TopicPath
        {
            get { return _topicPath; }
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