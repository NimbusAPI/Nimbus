using System;
using System.Linq;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Extensions
{
    public static class HandlerTypesProviderExtensions
    {
        public static Type[] AllHandlerTypes(this ITypeProvider typeProvider)
        {
            return new Type[0]
                .Union(typeProvider.CommandHandlerTypes)
                .Union(typeProvider.MulticastEventHandlerTypes)
                .Union(typeProvider.CompetingEventHandlerTypes)
                .Union(typeProvider.RequestHandlerTypes)
                .ToArray();
        }
    }
}