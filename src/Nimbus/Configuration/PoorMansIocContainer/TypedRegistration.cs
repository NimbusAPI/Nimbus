using System;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    internal class TypedRegistration : IComponentRegistration
    {
        public TypedRegistration(Type concreteType, Type implementedType, ComponentLifetime lifetime)
        {
            ConcreteType = concreteType;
            ImplementedType = implementedType;
            Lifetime = lifetime;
        }

        public Type ConcreteType { get; }
        public Type ImplementedType { get; }
        public ComponentLifetime Lifetime { get; }
    }
}