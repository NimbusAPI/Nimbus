using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly CommandHandlerTypesSetting _commandHandlerTypes;
        private readonly ICommandBroker _commandBroker;
        private readonly IQueueManager _queueManager;
        private readonly DefaultBatchSizeSetting _defaultBatchSize;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public CommandMessagePumpsFactory(ILogger logger,
                                          CommandHandlerTypesSetting commandHandlerTypes,
                                          ICommandBroker commandBroker,
                                          IQueueManager queueManager,
                                          DefaultBatchSizeSetting defaultBatchSize)
        {
            _logger = logger;
            _commandHandlerTypes = commandHandlerTypes;
            _commandBroker = commandBroker;
            _queueManager = queueManager;
            _defaultBatchSize = defaultBatchSize;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating command message pumps");

            var commandTypes = _commandHandlerTypes.Value.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleCommand<>)))
                                                   .Select(gi => gi.GetGenericArguments().First())
                                                   .OrderBy(t => t.FullName)
                                                   .Distinct()
                                                   .ToArray();

            foreach (var commandType in commandTypes)
            {
                _logger.Debug("Creating message pump for command type {0}", commandType.Name);

                var queuePath = PathFactory.QueuePathFor(commandType);
                var messageReceiver = new NimbusQueueMessageReceiver(_queueManager, queuePath);
                _garbageMan.Add(messageReceiver);

                var dispatcher = new CommandMessageDispatcher(_commandBroker, commandType);
                _garbageMan.Add(dispatcher);

                var pump = new MessagePump(messageReceiver, dispatcher, _logger, _defaultBatchSize);
                _garbageMan.Add(pump);

                yield return pump;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CommandMessagePumpsFactory()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}