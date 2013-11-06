using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public interface ITimeoutSender
    {
        Task Defer<TBusCommand>(DateTime proccessAt, TBusCommand busTimeout);
        Task Defer<TBusCommand>(TimeSpan delay, TBusCommand busTimeout);
    }
}