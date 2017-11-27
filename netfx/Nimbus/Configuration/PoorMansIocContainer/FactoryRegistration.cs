using System;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    internal class FactoryRegistration : IComponentRegistration
    {
        public FactoryRegistration(Func<PoorMansIoC, object> factory, Type concreteType, ComponentLifetime lifetime, Type implementedType)
        {
            ConcreteType = concreteType;
            Factory = factory;
            ImplementedType = implementedType;
            Lifetime = lifetime;
        }

        public Func<PoorMansIoC, object> Factory { get; }
        public Type ConcreteType { get; }
        public Type ImplementedType { get; }
        public ComponentLifetime Lifetime { get; }
    }
}