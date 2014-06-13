using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Extensions
{
    internal static class HandlerMapExtensions
    {
        internal static Type[] GetHandlerTypesFor(this IReadOnlyDictionary<Type, Type[]> handlerMap, Type messageType)
        {
            Type[] handlerTypes;
            if (!handlerMap.TryGetValue(messageType, out handlerTypes) || !handlerTypes.Any())
                throw new Exception("There is no handler registered for the message type {0}.".FormatWith(messageType));

            return handlerTypes;
        }

        internal static Type GetSingleHandlerTypeFor(this IReadOnlyDictionary<Type, Type[]> handlerMap, Type messageType)
        {
            var handlerTypes = handlerMap.GetHandlerTypesFor(messageType);
            if (handlerTypes.Length > 1)
                throw new NotSupportedException(
                    "Multiple handlers for the same message type on the same message pump are not supported. The message type {0} is handled by {1}.".FormatWith(
                        messageType,
                        handlerTypes.ToTypeNameSummary()));

            return handlerTypes.Single();
        }
    }
}