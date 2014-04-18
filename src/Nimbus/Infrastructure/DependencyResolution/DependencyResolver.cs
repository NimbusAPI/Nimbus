using System;
using System.Linq;
using Nimbus.ConcurrentCollections;
using Nimbus.DependencyResolution;

namespace Nimbus.Infrastructure.DependencyResolution
{
    public class DependencyResolver : TrackingScope, IDependencyResolver
    {
        private readonly ITypeProvider _typeProvider;
        private readonly ThreadSafeLazy<Type[]> _handlerTypes;

        public DependencyResolver(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _handlerTypes = new ThreadSafeLazy<Type[]>(ScanForHandlerTypes);
        }

        public IDependencyResolverScope CreateChildScope()
        {
            var childScope = new DependencyResolverScope(_handlerTypes.Value);
            Track(childScope);
            return childScope;
        }

        private Type[] ScanForHandlerTypes()
        {
            return new Type[0]
                .Union(_typeProvider.CommandHandlerTypes)
                .Union(_typeProvider.CompetingEventHandlerTypes)
                .Union(_typeProvider.MulticastEventHandlerTypes)
                .Union(_typeProvider.RequestHandlerTypes)
                .ToArray();
        }
    }
}