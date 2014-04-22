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

        public DependencyResolverScope(Type[] componentTypes)
        {
            _componentTypes = componentTypes;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            var childScope = new DependencyResolverScope(_componentTypes);
            Track(childScope);
            return childScope;
        }

        public TComponent Resolve<TComponent>()
        {
            var component = ComponentsOfType<TComponent>()
                .Select(t => (TComponent) CreateInstance(t))
                .First();

            Track(component);

            return component;
        }

        public TComponent Resolve<TComponent>(string componentName)
        {
            var component = ComponentsOfType<TComponent>()
                .Where(t => string.CompareOrdinal(t.FullName, componentName) == 0)
                .Select(t => (TComponent) CreateInstance(t))
                .First();

            Track(component);

            return component;
        }

        public object Resolve(Type componentType, string componentName)
        {
            var component = ComponentsOfType(componentType)
                .Where(t => string.CompareOrdinal(t.FullName, componentName) == 0)
                .Select(CreateInstance)
                .First();

            Track(component);

            return component;
        }

        private IEnumerable<Type> ComponentsOfType<TComponent>()
        {
            return ComponentsOfType(typeof (TComponent));
        }

        private IEnumerable<Type> ComponentsOfType(Type componentType)
        {
            //FIXME doesn't handle contravariance yet
            return _componentTypes
                .Where(componentType.IsAssignableFrom)
                .Where(t => t.IsInstantiable());
        }

        private object CreateInstance(Type implementingType)
        {
            try
            {
                var result = Activator.CreateInstance(implementingType);
                return result;
            }
            catch (Exception exc)
            {
                var message = (
                    "The {0} can only broker messages to handlers that have default constructors (i.e. ones with no parameters). " +
                    "If you'd like to use constructor injection on your handlers, have a look at the examples provided in the README " +
                    "about how to wire things up via an IoC container."
                    ).FormatWith(GetType().Name);

                throw new BusException(message, exc);
            }
        }
    }
}