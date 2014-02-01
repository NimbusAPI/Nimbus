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
        private readonly IQueueManager _queueManager;
        private readonly IClock _clock;
        private readonly HashSet<Type> _validCommandTypes;

        public BusCommandSender(IQueueManager queueManager, IClock clock, CommandTypesSetting validCommandTypes)
        {
            _queueManager = queueManager;
            _clock = clock;
            _validCommandTypes = new HashSet<Type>(validCommandTypes.Value);
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            if (!_validCommandTypes.Contains(typeof (TBusCommand)))
                throw new BusException(
                    "The type {0} is not a recognised command type. Ensure it has been registered with the builder with the WithTypesFrom method.".FormatWith(
                        typeof (TBusCommand).FullName));

            var sender = _queueManager.GetMessageSender(typeof (TBusCommand));
            var message = new BrokeredMessage(busCommand);
            await sender.Send(message);
        }

        public async Task SendAt<TBusCommand>(TimeSpan delay, TBusCommand busCommand)
        {
            await SendAt(_clock.UtcNow.Add(delay), busCommand);
        }

        public async Task SendAt<TBusCommand>(DateTimeOffset proccessAt, TBusCommand busCommand)
        {
            var sender = _queueManager.GetMessageSender(typeof (TBusCommand));
            var message = new BrokeredMessage(busCommand)
                          {
                              ScheduledEnqueueTimeUtc = proccessAt.DateTime
                          };

            await sender.Send(message);
        }
    }
}