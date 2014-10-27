using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.DependencyResolution
{
    public class DependencyResolverScope : TrackingScope, IDependencyResolverScope
    {
        private readonly Type[] _componentTypes;
        private readonly IDictionary<Type, object> _registeredInstances;

        public DependencyResolverScope(Type[] componentTypes, IDictionary<Type, object> registeredInstances)
        {
            _componentTypes = componentTypes;
            _registeredInstances = registeredInstances;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            var childScope = new DependencyResolverScope(_componentTypes, _registeredInstances);
            Track(childScope);
            return childScope;
        }

        public TComponent Resolve<TComponent>()
        {
            var componentType = typeof (TComponent);
            return (TComponent) Resolve(componentType);
        }

        public object Resolve(Type componentType)
        {
            object registeredInstance;
            if (_registeredInstances.TryGetValue(componentType, out registeredInstance)) return registeredInstance;

            var component = CreateInstance(componentType);
            Track(component);

            return component;
        }

        private object CreateInstance(Type componentType)
        {
            try
            {
                var args = componentType.GetConstructors()
                                        .Single()
                                        .GetParameters()
                                        .Select(p => Resolve(p.ParameterType))
                                        .ToArray();

                var result = Activator.CreateInstance(componentType, args);
                return result;
            }
            catch (Exception exc)
            {
                var message = (
                                  "The {0} can only broker messages to handlers that have default constructors (i.e. ones with no parameters) or with dependencies supplied directly via {0}.Register(...). " +
                                  "If you'd like to use constructor injection on your handlers, have a look at the examples provided in the README about how to wire things up via an IoC container."
                              ).FormatWith(GetType().Name);

                throw new BusException(message, exc);
            }
        }
    }
}