using System;
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
            var component = _componentTypes
                .Where(t => typeof (TComponent).IsAssignableFrom(t))
                .Where(t => t.IsInstantiable())
                .Select(t => (TComponent) CreateInstance(t))
                .First();

            Track(component);

            return component;
        }

        public TComponent Resolve<TComponent>(string componentName)
        {
            var component = _componentTypes
                .Where(t => typeof (TComponent).IsAssignableFrom(t))
                .Where(t => t.IsInstantiable())
                .Where(t => string.CompareOrdinal(t.FullName, componentName) == 0)
                .Select(t => (TComponent) CreateInstance(t))
                .First();

            Track(component);

            return component;
        }

        public TComponent[] ResolveAll<TComponent>()
        {
            throw new NotImplementedException();
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