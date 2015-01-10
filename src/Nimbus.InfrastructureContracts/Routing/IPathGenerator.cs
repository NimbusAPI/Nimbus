using System;

namespace Nimbus.Routing
{
    public interface IPathGenerator
    {
        string InputQueuePathFor(string applicationName, string instanceName);
        string QueuePathFor(Type type);
        string TopicPathFor(Type type);
        string SubscriptionNameFor(string applicationName);
        string SubscriptionNameFor(string applicationName, string instanceName);
        string SubscriptionNameFor(string applicationName, Type handlerType);
        string SubscriptionNameFor(string applicationName, string instanceName, Type handlerType);
    }
}