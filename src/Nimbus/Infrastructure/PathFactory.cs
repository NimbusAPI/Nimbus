using System;
using Nimbus.Configuration;

namespace Nimbus.Infrastructure
{
    public static class PathFactory
    {
        static PathFactory()
        {
            Provider = new TypeConventionPathFactoryProvider();
        }

        public static IPathFactoryProvider Provider { get; set; }

        public static string InputQueuePathFor(string applicationName, string instanceName)
        {
            return Provider.InputQueuePathFor(applicationName, instanceName);
        }

        public static string QueuePathFor(Type type)
        {
            return Provider.QueuePathFor(type);
        }

        public static string TopicPathFor(Type type)
        {
            return Provider.TopicPathFor(type);
        }

        public static string SubscriptionNameFor(string applicationName)
        {
            return Provider.SubscriptionNameFor(applicationName);
        }

        public static string SubscriptionNameFor(string applicationName, string instanceName)
        {
            return Provider.SubscriptionNameFor(applicationName, instanceName);
        }

        public static string SubscriptionNameFor(string applicationName, Type handlerType)
        {
            return Provider.SubscriptionNameFor(applicationName, handlerType);
        }

        public static string SubscriptionNameFor(string applicationName, string instanceName, Type handlerType)
        {
            return Provider.SubscriptionNameFor(applicationName, instanceName, handlerType);
        }
    }
}