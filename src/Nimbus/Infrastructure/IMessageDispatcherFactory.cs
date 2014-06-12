using System;
using System.Collections.Generic;

namespace Nimbus.Infrastructure
{
    internal interface IMessageDispatcherFactory
    {
        IMessageDispatcher Create(Type handlerType, IReadOnlyDictionary<Type, Type> handlerMap);
    }
}