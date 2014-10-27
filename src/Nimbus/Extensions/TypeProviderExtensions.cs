using System;
using System.Linq;
using Nimbus.Handlers;

namespace Nimbus.Extensions
{
    public static class TypeProviderExtensions
    {
        public static Type[] AllResolvableTypes(this ITypeProvider typeProvider)
        {
            return typeProvider.AllHandlerTypes().Union(typeProvider.InterceptorTypes).ToArray();
        }

        public static Type[] AllHandlerTypes(this ITypeProvider typeProvider)
        {
            return new Type[0]
                .Union(typeProvider.CommandHandlerTypes)
                .Union(typeProvider.MulticastEventHandlerTypes)
                .Union(typeProvider.CompetingEventHandlerTypes)
                .Union(typeProvider.RequestHandlerTypes)
                .Union(typeProvider.MulticastRequestHandlerTypes)
                .ToArray();
        }

        public static Type[] AllClosedGenericHandlerInterfaces(this ITypeProvider typeProvider)
        {
            return typeProvider.AllHandlerTypes()
                               .SelectMany(t => t.GetInterfaces())
                               .Where(typeProvider.IsClosedGenericHandlerInterface)
                               .ToArray();
        }

        public static Type[] AllTypesHandledViaTopics(this ITypeProvider typeProvider)
        {
            var handlers = new Type[0]
                .Union(typeProvider.MulticastEventHandlerTypes)
                .Union(typeProvider.CompetingEventHandlerTypes)
                .Union(typeProvider.MulticastRequestHandlerTypes)
                .ToArray();

            var handledEvents = handlers.SelectMany(hand => hand.GetInterfaces())
                                        .Where(
                                            i =>
                                            i.IsClosedTypeOf(typeof (IHandleCompetingEvent<>)) ||
                                            i.IsClosedTypeOf(typeof (IHandleMulticastEvent<>)) ||
                                            i.IsClosedTypeOf(typeof (IHandleMulticastRequest<,>))
                )
                                        .SelectMany(i => i.GetGenericArguments());

            return handledEvents.Distinct().ToArray();
        }

        public static Type[] AllHandledEventTypes(this ITypeProvider typeProvider)
        {
            var handlers = new Type[0]
                .Union(typeProvider.MulticastEventHandlerTypes)
                .Union(typeProvider.CompetingEventHandlerTypes)
                .ToArray();

            var handledEvents = handlers.SelectMany(hand => hand.GetInterfaces())
                                        .Where(i => i.IsClosedTypeOf(typeof (IHandleCompetingEvent<>)) || i.IsClosedTypeOf(typeof (IHandleMulticastEvent<>)))
                                        .SelectMany(i => i.GetGenericArguments());

            return handledEvents.Distinct().ToArray();
        }

        public static Type[] AllMessageContractTypes(this ITypeProvider typeProvider)
        {
            return new Type[0]
                .Union(typeProvider.CommandTypes)
                .Union(typeProvider.EventTypes)
                .Union(typeProvider.RequestTypes)
                .Union(typeProvider.ResponseTypes)
                .Union(typeProvider.MulticastRequestTypes)
                .Union(typeProvider.MulticastResponseTypes)
                .ToArray();
        }

        public static bool IsClosedGenericHandlerInterface(this ITypeProvider typeProvider, Type potentialHandlerType)
        {
            if (!potentialHandlerType.IsInterface) return false;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleCommand<>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleMulticastEvent<>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleCompetingEvent<>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleRequest<,>))) return true;
            if (potentialHandlerType.IsClosedTypeOf(typeof (IHandleMulticastRequest<,>))) return true;
            return false;
        }
    }
}