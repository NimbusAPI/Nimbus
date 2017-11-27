using System;
using System.Collections.Generic;

namespace Nimbus.Infrastructure
{
    internal interface IHandlerMapper
    {
        IReadOnlyDictionary<Type, Type[]> GetFullHandlerMap(Type openGenericHandlerType);
        IEnumerable<Type> GetMessageTypesHandledBy(Type openGenericHandlerType, Type handlerType);
        IEnumerable<Type> GetMessageTypesHandledBy(Type openGenericHandlerType, IEnumerable<Type> handlerTypes);
        IReadOnlyDictionary<Type, Type[]> GetHandlerMapFor(Type openGenericHandlerType, IEnumerable<Type> messageTypes);
        Type[] GetHandlerTypesFor(Type openGenericHandlerType, Type messageType);
    }
}