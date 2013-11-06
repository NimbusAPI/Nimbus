using System;
using System.Linq;

namespace Nimbus.Configuration
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