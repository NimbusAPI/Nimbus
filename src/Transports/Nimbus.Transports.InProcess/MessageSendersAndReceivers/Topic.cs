using System.Linq;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class Topic
    {
        private readonly ThreadSafeDictionary<string, Queue> _subscriptionQueues = new ThreadSafeDictionary<string, Queue>();

        public Queue GetSubscriptionQueue(string subscriptionName)
        {
            return _subscriptionQueues.GetOrAdd(subscriptionName, p => new Queue());
        }

        public Queue[] SubscriptionQueues
        {
            get { return _subscriptionQueues.ToDictionary().Values.ToArray(); }
        }
    }
}