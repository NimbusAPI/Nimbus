using System;
using System.Linq;

namespace Nimbus.Configuration
{
    public static class HandlerTypesProviderExtensions
    {
        public static Type[] AllHandlerTypes(this ITypeProvider typeProvider)
        {
            return typeProvider.CommandHandlerTypes
                .Union(typeProvider.TimeoutHandlerTypes)
                                       .Union(typeProvider.EventHandlerTypes)
                                       .Union(typeProvider.RequestHandlerTypes)
                                       .ToArray();
        }
    }
}