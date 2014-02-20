using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public interface ICreateMessageBroker<TSubject> : IDisposable
    {
        Task<TSubject> Create(ITypeProvider typeProvider);
    }
}