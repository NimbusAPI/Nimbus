using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly ICommandBroker _commandBroker;
        private readonly Type _commandType;

        public CommandMessageDispatcher(ICommandBroker commandBroker, Type commandType)
        {
            _commandBroker = commandBroker;
            _commandType = commandType;
        }

        public Task Dispatch(BrokeredMessage message)
        {
            return Task.Run(() =>
                            {
                                var busCommand = message.GetBody(_commandType);
                                _commandBroker.Dispatch((dynamic) busCommand);
                            });
        }
    }
}