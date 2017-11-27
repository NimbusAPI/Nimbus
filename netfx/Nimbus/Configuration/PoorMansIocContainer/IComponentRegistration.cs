using System;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    internal interface IComponentRegistration
    {
        Type ConcreteType { get; }
        Type ImplementedType { get; }
        ComponentLifetime Lifetime { get; }
    }
}