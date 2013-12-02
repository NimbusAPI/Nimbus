using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Timeouts
{
    internal interface ITimeoutSender
    {
        Task Defer<TBusTimeout>(TimeSpan delay, IBusTimeout busTimeout);

        Task Defer<TBusTimeout>(DateTime processAt, IBusTimeout busTimeout);
    }
}