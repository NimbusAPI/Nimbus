using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class CommandMessagePumpsFactory
    {
        private readonly ILogger _logger;
        private readonly CommandHandlerTypesSetting _commandHandlerTypes;
        private readonly ICommandBroker _commandBroker;
        private readonly IQueueManager _queueManager;

        public CommandMessagePumpsFactory(ILogger logger, CommandHandlerTypesSetting commandHandlerTypes, ICommandBroker commandBroker, IQueueManager queueManager)
        {
            _logger = logger;
            _commandHandlerTypes = commandHandlerTypes;
            _commandBroker = commandBroker;
            _queueManager = queueManager;
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

                var dispatcher = new CommandMessageDispatcher(_commandBroker, commandType);
                var pump = new MessagePump(messageReceiver, dispatcher, _logger);

                yield return pump;
            }
        }
    }
}