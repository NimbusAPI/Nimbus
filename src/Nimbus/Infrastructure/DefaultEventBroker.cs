using System;
using System.Reflection;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public class DefaultEventBroker : IEventBroker
    {
        private readonly Assembly[] _assemblies;

        public DefaultEventBroker(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public void Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            throw new NotImplementedException();
        }
    }
}