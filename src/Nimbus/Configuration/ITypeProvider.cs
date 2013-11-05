using System;
using System.Collections.Generic;

namespace Nimbus.Configuration
{
    public interface ITypeProvider
    {
        IEnumerable<Type> CommandHandlerTypes { get; }
        IEnumerable<Type> TimeoutHandlerTypes { get; }
        IEnumerable<Type> EventHandlerTypes { get; }
        IEnumerable<Type> RequestHandlerTypes { get; }
    }
}