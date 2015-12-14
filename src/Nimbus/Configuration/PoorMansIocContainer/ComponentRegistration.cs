using System;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    internal class ComponentRegistration
    {
        private readonly Type _concreteType;
        private readonly Type _implementedType;
        private readonly ComponentLifetime _lifetime;

        public ComponentRegistration(Type concreteType, Type implementedType, ComponentLifetime lifetime)
        {
            _concreteType = concreteType;
            _implementedType = implementedType;
            _lifetime = lifetime;
        }

        public Type ConcreteType
        {
            get { return _concreteType; }
        }

        public Type ImplementedType
        {
            get { return _implementedType; }
        }

        public ComponentLifetime Lifetime
        {
            get { return _lifetime; }
        }
    }
}