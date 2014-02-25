using System;
using System.Threading.Tasks;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public interface ICreateMessageHandlerFactory<TSubject> : IDisposable
    {
        Task<TSubject> Create(ITypeProvider typeProvider);
    }
}