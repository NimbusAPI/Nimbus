using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal interface ICommandSender
    {
        Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand;

        Task Send(IBusCommand busCommand);

        Task SendAt<TBusCommand>(TimeSpan delay, TBusCommand busCommand);

        Task SendAt<TBusCommand>(DateTimeOffset proccessAt, TBusCommand busCommand);
    }
}