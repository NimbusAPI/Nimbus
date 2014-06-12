using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure
{
    internal class HandlerMapper : IHandlerMapper
    {
        private readonly ITypeProvider _typeProvider;
        private readonly ThreadSafeLazy<Dictionary<Type, Type>> _handlerMap;

        public HandlerMapper(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _handlerMap = new ThreadSafeLazy<Dictionary<Type, Type>>(Build);
        }

        public bool TryGetHandlerTypeFor(Type messageType, out Type handlerType)
        {
            return _handlerMap.Value.TryGetValue(messageType, out handlerType);
        }

        public IReadOnlyDictionary<Type, Type> GetHandlerMapFor(IEnumerable<Type> messageTypes)
        {
            return _handlerMap.Value.Where(x => messageTypes.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }

        public IReadOnlyDictionary<Type, Type> GetFullHandlerMap()
        {
            return _handlerMap.Value;
        }

        private Dictionary<Type, Type> Build()
        {
            var commandHandlerMap = BuildHandlerMap(_typeProvider.CommandHandlerTypes, typeof(IHandleCommand<>));
            var competingEventHandlerMap = BuildHandlerMap(_typeProvider.CompetingEventHandlerTypes, typeof(IHandleCompetingEvent<>));
            var multicastEventHandlerMap = BuildHandlerMap(_typeProvider.MulticastEventHandlerTypes, typeof(IHandleMulticastEvent<>));
            var requestHandlerMap = BuildHandlerMap(_typeProvider.RequestHandlerTypes, typeof(IHandleRequest<,>));
            var multicastRequestHandlerMap = BuildHandlerMap(_typeProvider.MulticastRequestHandlerTypes, typeof(IHandleMulticastRequest<,>));

            var map = commandHandlerMap.Concat(competingEventHandlerMap)
                                       .Concat(multicastEventHandlerMap)
                                       .Concat(requestHandlerMap)
                                       .Concat(multicastRequestHandlerMap)
                                       .ToDictionary(x => x.Key, x => x.Value);

            //AssertSingleHandlers(map);

            return map.ToDictionary(x => x.Key, x => x.Value.Single());
        }

        private IEnumerable<KeyValuePair<Type, Type[]>> BuildHandlerMap(IEnumerable<Type> handlerTypes, Type openGenericHandlerType)
        {
            var handlers = handlerTypes as Type[] ?? handlerTypes.ToArray();
            var messages = GetMessageTypesHandledBy(handlers, openGenericHandlerType);
            var handlerMap = MapMessagesToHandlers(messages, handlers, openGenericHandlerType);

            return handlerMap;
        }

        private IEnumerable<Type> GetMessageTypesHandledBy(IEnumerable<Type> handlerTypes, Type openGenericHandlerType)
        {
            return handlerTypes.SelectMany(h => h.GetGenericInterfacesClosing(openGenericHandlerType).Select(gi => gi.GetGenericArguments().First())).OrderBy(m => m.Name).Distinct();
        }

        private IEnumerable<KeyValuePair<Type, Type[]>> MapMessagesToHandlers(IEnumerable<Type> messageTypes, IEnumerable<Type> handlerTypes, Type openGenericHandlerType)
        {
            return messageTypes.ToDictionary(
                m => m,
                m => handlerTypes.Where(h => h.GetGenericInterfacesClosing(openGenericHandlerType).Select(gi => gi.GetGenericArguments().First()).Contains(m)).ToArray());
        }

        private static void AssertSingleHandlers(IEnumerable<KeyValuePair<Type, Type[]>> map)
        {
            var mappingsWithMultipleHandlers = map.Where(x => x.Value.Length > 1).ToArray();
            if (mappingsWithMultipleHandlers.Any())
            {
                var description = string.Join(Environment.NewLine, mappingsWithMultipleHandlers.Select(m => "{0}: {1}".FormatWith(m.Key, string.Join(Environment.NewLine + "\t", m.Value.Select(t => t.FullName)))).ToArray());
                throw new NotSupportedException("The following messages have multiple handlers registered: {0}{1}".FormatWith(Environment.NewLine, description));
            }
        }
    }
}