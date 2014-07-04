using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.ConcurrentCollections;
using Nimbus.DependencyResolution;

namespace Nimbus.Infrastructure.DependencyResolution
{
    public class DependencyResolver : TrackingScope, IDependencyResolver
    {
        private readonly ITypeProvider _typeProvider;
        private readonly ThreadSafeLazy<Type[]> _resolvableTypes;
        private readonly Dictionary<Type, object> _additionalComponents = new Dictionary<Type, object>();  

        public DependencyResolver(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _resolvableTypes = new ThreadSafeLazy<Type[]>(ScanForResolvableTypes);
        }

        public void Register(object instance, params Type[] asTypes)
        {
            foreach (var type in asTypes) _additionalComponents[type] = instance;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            var childScope = new DependencyResolverScope(_resolvableTypes.Value, _additionalComponents.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            Track(childScope);
            return childScope;
        }

        private Type[] ScanForResolvableTypes()
        {
            return new Type[0]
                .Union(_typeProvider.CommandHandlerTypes)
                .Union(_typeProvider.CompetingEventHandlerTypes)
                .Union(_typeProvider.MulticastEventHandlerTypes)
                .Union(_typeProvider.RequestHandlerTypes)
                .Union(_typeProvider.MulticastRequestHandlerTypes)
                .Union(_typeProvider.InterceptorTypes)
                .ToArray();
        }
    }
}