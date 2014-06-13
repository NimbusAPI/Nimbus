using System;
using System.Collections.Generic;

namespace Nimbus.Infrastructure
{
    internal interface IMessageDispatcherFactory
    {
        IMessageDispatcher Create(Type openGenericHandlerType, IReadOnlyDictionary<Type, Type[]> handlerMap);
    }
}