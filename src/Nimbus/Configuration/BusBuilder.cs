using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.MessagePumps;

namespace Nimbus.Configuration
{
    public class BusBuilder
    {
        private readonly BusBuilderConfiguration _configuration;

        public BusBuilder()
        {
            _configuration = new BusBuilderConfiguration(this);
        }

        public BusBuilderConfiguration Configure()
        {
            return _configuration;
        }

        internal Bus Build()
        {
            var replyQueueName = _configuration.InstanceName;

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_configuration.ConnectionString);
            var messagingFactory = MessagingFactory.CreateFromConnectionString(_configuration.ConnectionString);
            var queueManager = new QueueManager(namespaceManager);
            var messagePumps = new List<IMessagePump>();
            var requestResponseCorrelator = new RequestResponseCorrelator();
            var messageSenderFactory = new MessageSenderFactory(messagingFactory);
            var topicClientFactory = new TopicClientFactory(messagingFactory);
            var commandSender = new BusCommandSender(messageSenderFactory);
            var requestSender = new BusRequestSender(messageSenderFactory, replyQueueName, requestResponseCorrelator, _configuration.DefaultTimeout);
            var eventSender = new BusEventSender(topicClientFactory);

            CreateRequestResponseMessagePump(messagingFactory, queueManager, replyQueueName, requestResponseCorrelator, messagePumps);
            CreateCommandMessagePumps(queueManager, messagingFactory, messagePumps);
            CreateRequestMessagePumps(queueManager, messagingFactory, messagePumps);
            CreateEventMessagePumps(queueManager, messagingFactory, messagePumps);

            var bus = new Bus(commandSender, requestSender, eventSender, messagePumps);
            return bus;
        }

        private void CreateRequestResponseMessagePump(MessagingFactory messagingFactory,
                                                      IQueueManager queueManager,
                                                      string replyQueueName,
                                                      RequestResponseCorrelator requestResponseCorrelator,
                                                      List<IMessagePump> messagePumps)
        {
            queueManager.EnsureQueueExists(replyQueueName);
            var requestResponseMessagePump = new ResponseMessagePump(messagingFactory, replyQueueName, requestResponseCorrelator);
            messagePumps.Add(requestResponseMessagePump);
        }

        private void CreateEventMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var eventType in ExtractDistinctGenericTypeArguments(_configuration.EventHandlerTypes, typeof (IHandleEvent<>)))
            {
                queueManager.EnsureTopicExists(eventType);
                var subscriptionName = String.Format("{0}.{1}", Environment.MachineName, "MyApp");
                queueManager.EnsureSubscriptionExists(eventType, subscriptionName);

                var pump = new EventMessagePump(messagingFactory, _configuration.EventBroker, eventType, subscriptionName);
                messagePumps.Add(pump);
            }
        }

        private void CreateRequestMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var requestType in ExtractDistinctGenericTypeArguments(_configuration.RequestHandlerTypes, typeof (IHandleRequest<,>)))
            {
                queueManager.EnsureQueueExists(requestType);

                var pump = new RequestMessagePump(messagingFactory, _configuration.RequestBroker, requestType);
                messagePumps.Add(pump);
            }
        }

        private void CreateCommandMessagePumps(IQueueManager queueManager, MessagingFactory messagingFactory, List<IMessagePump> messagePumps)
        {
            foreach (var commandType in ExtractDistinctGenericTypeArguments(_configuration.CommandHandlerTypes, typeof (IHandleCommand<>)))
            {
                queueManager.EnsureQueueExists(commandType);

                var pump = new CommandMessagePump(messagingFactory, _configuration.CommandBroker, commandType);
                messagePumps.Add(pump);
            }
        }

        private static IEnumerable<Type> ExtractDistinctGenericTypeArguments(IEnumerable<Type> handlerTypes, Type openGenericType)
        {
            return handlerTypes.SelectMany(t => t.GetInterfaces())
                               .Where(interfaceType => interfaceType.IsClosedTypeOf(openGenericType))
                               .Select(genericInterfaceType => genericInterfaceType.GetGenericArguments()[0])
                               .Distinct()
                               .ToArray();
        }
    }
}