using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.Retries
{
    internal interface IRetry
    {
        void Do(Action action, string actionName = "");
        T Do<T>(Func<T> func, string actionName = "");
        Task DoAsync(Func<Task> action, string actionName = "");
        Task<T> DoAsync<T>(Func<Task<T>> func, string actionName = "");
    }
}