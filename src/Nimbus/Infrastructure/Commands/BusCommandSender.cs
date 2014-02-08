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
        private readonly INimbusMessageSenderFactory _messageSenderFactory;
        private readonly IClock _clock;
        private readonly HashSet<Type> _validCommandTypes;

        public BusCommandSender(INimbusMessageSenderFactory messageSenderFactory, IClock clock, CommandTypesSetting validCommandTypes)
        {
            _messageSenderFactory = messageSenderFactory;
            _clock = clock;
            _validCommandTypes = new HashSet<Type>(validCommandTypes.Value);
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            AssertValidCommandType<TBusCommand>();

            var message = new BrokeredMessage(busCommand);

            var sender = _messageSenderFactory.GetQueueSender(typeof (TBusCommand));
            await sender.Send(message);
        }

        public async Task SendAt<TBusCommand>(TimeSpan delay, TBusCommand busCommand)
        {
            await SendAt(_clock.UtcNow.Add(delay), busCommand);
        }

        public async Task SendAt<TBusCommand>(DateTimeOffset proccessAt, TBusCommand busCommand)
        {
            AssertValidCommandType<TBusCommand>();

            var message = new BrokeredMessage(busCommand)
                          {
                              ScheduledEnqueueTimeUtc = proccessAt.DateTime
                          };

            var sender = _messageSenderFactory.GetQueueSender(typeof(TBusCommand));
            await sender.Send(message);
        }

        private void AssertValidCommandType<TBusCommand>()
        {
            if (!_validCommandTypes.Contains(typeof (TBusCommand)))
                throw new BusException(
                    "The type {0} is not a recognised command type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TBusCommand).FullName));
        }
    }
}