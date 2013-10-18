using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly string _connectionString;
        private readonly ICommandBroker _commandBroker;
        private readonly IRequestBroker _requestBroker;
        private readonly Type[] _commandTypes;
        private readonly Type[] _requestTypes;
        private MessagingFactory _messagingFactory;
        private readonly IList<IMessagePump> _messagePumps = new List<IMessagePump>();
        private IRequestResponseCorrelator _correlator;
        private string _replyQueueName;

        public Bus(string connectionString, ICommandBroker commandBroker, IRequestBroker requestBroker, Type[] commandTypes, Type[] requestTypes)
        {
            _connectionString = connectionString;
            _commandBroker = commandBroker;
            _requestBroker = requestBroker;
            _commandTypes = commandTypes;
            _requestTypes = requestTypes;
        }

        public void Send(object busCommand)
        {
            var sender = _messagingFactory.CreateMessageSender(busCommand.GetType().FullName);
            var message = new BrokeredMessage(busCommand);
            sender.Send(message);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var response = await _correlator.MakeCorrelatedRequest(busRequest);
            return response;
        }

        public void Start()
        {
            _messagingFactory = MessagingFactory.CreateFromConnectionString(_connectionString);
            var customQueueName = "DefaultQueue";
            _replyQueueName = string.Format("{0}.{1}.{2}", Environment.MachineName, Process.GetCurrentProcess().ProcessName, customQueueName);
            _correlator = new RequestResponseCorrelator(_messagingFactory, _replyQueueName);

            EnsureQueueExists(_replyQueueName);

            foreach (var commandType in _commandTypes)
            {
                EnsureQueueExists(commandType);

                var pump = new MessagePump(_messagingFactory, _commandBroker, commandType);
                _messagePumps.Add(pump);
                pump.Start();
            }

            foreach (var requestType in _requestTypes)
            {
                EnsureQueueExists(requestType);

                var pump = new RequestMessagePump(_messagingFactory, _requestBroker, requestType);
                _messagePumps.Add(pump);
                pump.Start();
            }

            _correlator.Start();
        }

        private void EnsureQueueExists(Type commandType)
        {
            EnsureQueueExists(commandType.FullName);
        }

        private void EnsureQueueExists(string queueName)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            if (!namespaceManager.QueueExists(queueName))
            {
                namespaceManager.CreateQueue(queueName);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}