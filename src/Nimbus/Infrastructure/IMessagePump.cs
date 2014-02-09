using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IMessagePump: IDisposable
    {
        Task Start();
        Task Stop();
    }
}