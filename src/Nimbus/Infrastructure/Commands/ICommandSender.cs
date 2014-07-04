using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal interface ICommandSender
    {
        Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand;
        Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset whenToSend) where TBusCommand : IBusCommand;
    }
}