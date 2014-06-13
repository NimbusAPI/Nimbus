using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure
{
    internal class HandlerMapper : IHandlerMapper, IValidatableConfigurationSetting
    {
        private readonly ITypeProvider _typeProvider;
        private readonly ThreadSafeLazy<IReadOnlyDictionary<Type, IReadOnlyDictionary<Type, Type[]>>> _handlerMap;

        public HandlerMapper(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _handlerMap = new ThreadSafeLazy<IReadOnlyDictionary<Type, IReadOnlyDictionary<Type, Type[]>>>(Build);
        }

        public Type[] GetHandlerTypesFor(Type openGenericHandlerType, Type messageType)
        {
            return _handlerMap.Value[openGenericHandlerType][messageType];
        }

        public IReadOnlyDictionary<Type, Type[]> GetHandlerMapFor(Type openGenericHandlerType, IEnumerable<Type> messageTypes)
        {
            return _handlerMap.Value[openGenericHandlerType].Where(x => messageTypes.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }

        public IReadOnlyDictionary<Type, Type[]> GetFullHandlerMap(Type openGenericHandlerType)
        {
            return _handlerMap.Value[openGenericHandlerType];
        }

        public IEnumerable<Type> GetMessageTypesHandledBy(Type openGenericHandlerType, Type handlerType)
        {
            return GetMessageTypesHandledBy(openGenericHandlerType, new[] {handlerType});
        }

        public IEnumerable<Type> GetMessageTypesHandledBy(Type openGenericHandlerType, IEnumerable<Type> handlerTypes)
        {
            return handlerTypes
                .SelectMany(h => h.GetGenericInterfacesClosing(openGenericHandlerType).Select(gi => gi.GetGenericArguments().First()))
                .OrderBy(m => m.Name)
                .Distinct();
        }

        private IReadOnlyDictionary<Type, IReadOnlyDictionary<Type, Type[]>> Build()
        {
            var commandHandlerMap = BuildHandlerMap(_typeProvider.CommandHandlerTypes, typeof(IHandleCommand<>));
            var competingEventHandlerMap = BuildHandlerMap(_typeProvider.CompetingEventHandlerTypes, typeof(IHandleCompetingEvent<>));
            var multicastEventHandlerMap = BuildHandlerMap(_typeProvider.MulticastEventHandlerTypes, typeof(IHandleMulticastEvent<>));
            var requestHandlerMap = BuildHandlerMap(_typeProvider.RequestHandlerTypes, typeof(IHandleRequest<,>));
            var multicastRequestHandlerMap = BuildHandlerMap(_typeProvider.MulticastRequestHandlerTypes, typeof(IHandleMulticastRequest<,>));

            var map = new Dictionary<Type, IReadOnlyDictionary<Type, Type[]>>
                      {
                          {commandHandlerMap.Key, commandHandlerMap.Value},
                          {competingEventHandlerMap.Key, competingEventHandlerMap.Value},
                          {multicastEventHandlerMap.Key, multicastEventHandlerMap.Value},
                          {requestHandlerMap.Key, requestHandlerMap.Value},
                          {multicastRequestHandlerMap.Key, multicastRequestHandlerMap.Value},
                      };

            return map;
        }

        private KeyValuePair<Type, IReadOnlyDictionary<Type, Type[]>> BuildHandlerMap(IEnumerable<Type> handlerTypes, Type openGenericHandlerType)
        {
            var handlers = handlerTypes as Type[] ?? handlerTypes.ToArray();
            var messages = GetMessageTypesHandledBy(openGenericHandlerType, handlers);
            var handlerMap = MapMessagesToHandlers(openGenericHandlerType, messages, handlers);

            return new KeyValuePair<Type, IReadOnlyDictionary<Type, Type[]>>(openGenericHandlerType, handlerMap);
        }

        private IReadOnlyDictionary<Type, Type[]> MapMessagesToHandlers(Type openGenericHandlerType, IEnumerable<Type> messageTypes, IEnumerable<Type> handlerTypes)
        {
            return messageTypes.ToDictionary(
                m => m,
                m => handlerTypes
                    .Where(h => h.GetGenericInterfacesClosing(openGenericHandlerType).Select(gi => gi.GetGenericArguments().First()).Contains(m))
                    .ToArray());
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

        public IEnumerable<string> Validate()
        {
            var validationErrors = new string[0]
                .Union(CheckForUnsupportedMultipleHandlers())
                .ToArray();

            return validationErrors;
        }

        private IEnumerable<string> CheckForUnsupportedMultipleHandlers()
        {
            var validationErrors =
                _handlerMap.Value
                           .Where(genericMapping => genericMapping.Key == typeof (IHandleCommand<>) || genericMapping.Key == typeof (IHandleRequest<,>))
                           .SelectMany(genericMapping => genericMapping.Value.ToArray())
                           .Where(mapping => mapping.Value.Count() > 1)
                           .Select(mapping => "The message contract type {0} has multiple handlers: {1}".FormatWith(mapping.Key, mapping.Value.ToTypeNameSummary()))
                           .ToArray();

            return validationErrors;
        }

    }
}