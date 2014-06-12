using System;
using System.Collections.Generic;

namespace Nimbus.Infrastructure
{
    internal interface IHandlerMapper
    {
        bool TryGetHandlerTypeFor(Type messageType, out Type handlerType);
        IReadOnlyDictionary<Type, Type> GetHandlerMapFor(IEnumerable<Type> messageTypes);
        IReadOnlyDictionary<Type, Type> GetFullHandlerMap();
    }
}