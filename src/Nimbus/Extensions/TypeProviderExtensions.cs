using System;
using System.Linq;
using Nimbus.Handlers;

namespace Nimbus.Extensions
{
    public static class TypeProviderExtensions
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

        public static Type[] AllClosedGenericHandlerInterfaces(this ITypeProvider typeProvider)
        {
            return typeProvider.AllHandlerTypes()
                               .Where(typeProvider.IsClosedGenericHandlerInterface)
                               .ToArray();
        }

        public static Type[] AllMessageContractTypes(this ITypeProvider typeProvider)
        {
            return new Type[0]
                .Union(typeProvider.CommandTypes)
                .Union(typeProvider.EventTypes)
                .Union(typeProvider.RequestTypes)
                .ToArray();
        }

        public static bool IsClosedGenericHandlerInterface(this ITypeProvider typeProvider, Type potentialHandlerType)
        {
            if (!potentialHandlerType.IsInterface) return false;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleCommand<>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleMulticastEvent<>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleCompetingEvent<>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleRequest<,>))) return true;
            return false;
        }
    }
}