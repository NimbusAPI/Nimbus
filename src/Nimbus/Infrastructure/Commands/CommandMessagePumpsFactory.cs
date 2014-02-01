using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class CommandMessagePumpsFactory
    {
        private readonly ILogger _logger;
        private readonly CommandHandlerTypesSetting _commandHandlerTypes;
        private readonly MessagingFactory _messagingFactory;
        private readonly ICommandBroker _commandBroker;

        public CommandMessagePumpsFactory(ILogger logger, CommandHandlerTypesSetting commandHandlerTypes, MessagingFactory messagingFactory, ICommandBroker commandBroker)
        {
            _logger = logger;
            _commandHandlerTypes = commandHandlerTypes;
            _messagingFactory = messagingFactory;
            _commandBroker = commandBroker;
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

                var queueName = PathFactory.QueuePathFor(commandType);
                var messageReceiver = new NimbusMessageReceiver(_messagingFactory.CreateMessageReceiver(queueName));

                var dispatcher = new CommandMessageDispatcher(_commandBroker, commandType);
                var pump = new MessagePump(messageReceiver, dispatcher, _logger);

                yield return pump;
            }
        }
    }
}