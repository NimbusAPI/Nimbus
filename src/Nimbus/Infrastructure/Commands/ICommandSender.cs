using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.Commands
{
    internal interface ICommandSender
    {
        Task Send<TBusCommand>(TBusCommand busCommand);

        Task SendAt<TBusCommand>(TimeSpan delay, TBusCommand busCommand);

        Task SendAt<TBusCommand>(DateTimeOffset proccessAt, TBusCommand busCommand);
    }
}