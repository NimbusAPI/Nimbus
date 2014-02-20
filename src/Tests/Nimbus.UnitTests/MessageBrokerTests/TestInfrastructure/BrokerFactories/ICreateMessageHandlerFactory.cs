using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public interface ICreateMessageHandlerFactory<TSubject> : IDisposable
    {
        Task<TSubject> Create(ITypeProvider typeProvider);
    }
}