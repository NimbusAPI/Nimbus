using System;
using System.Reflection;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public class DefaultMulticastEventBroker : IMulticastEventBroker
    {
        private readonly Assembly[] _assemblies;

        public DefaultMulticastEventBroker(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public void Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            throw new NotImplementedException();
        }
    }
}