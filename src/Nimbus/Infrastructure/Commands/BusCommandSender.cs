using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.Commands
{
    internal class BusCommandSender : ICommandSender
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IClock _clock;
        private readonly HashSet<Type> _validCommandTypes;

        public BusCommandSender(INimbusMessagingFactory messagingFactory, IClock clock, CommandTypesSetting validCommandTypes)
        {
            _messagingFactory = messagingFactory;
            _clock = clock;
            _validCommandTypes = new HashSet<Type>(validCommandTypes.Value);
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            var commandType = busCommand.GetType();
            AssertValidCommandType(commandType);

            var message = ConstructBrokeredMessage(busCommand, commandType);

            await Deliver(commandType, message);
        }

        public async Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset whenToSend)
        {
            var commandType = busCommand.GetType();
            AssertValidCommandType(commandType);

            var message = ConstructBrokeredMessage(busCommand, commandType);
            message.ScheduledEnqueueTimeUtc = whenToSend.DateTime;

            await Deliver(commandType, message);
        }

        private void AssertValidCommandType(Type commandType)
        {
            if (!_validCommandTypes.Contains(commandType))
                throw new BusException(
                    "The type {0} is not a recognised command type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        commandType.FullName));
        }

        private static BrokeredMessage ConstructBrokeredMessage<TBusCommand>(TBusCommand busCommand, Type commandType)
        {
            var message = new BrokeredMessage(busCommand);
            message.Properties[MessagePropertyKeys.MessageType] = commandType.AssemblyQualifiedName;
            return message;
        }

        private async Task Deliver(Type commandType, BrokeredMessage message)
        {
            var queuePath = PathFactory.QueuePathFor(commandType);
            var sender = _messagingFactory.GetQueueSender(queuePath);
            await sender.Send(message);
        }
    }
}