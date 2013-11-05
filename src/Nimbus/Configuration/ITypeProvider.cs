using System;
using System.Collections.Generic;

namespace Nimbus.Configuration
{
    public interface ITypeProvider
    {
        IEnumerable<Type> CommandHandlerTypes { get; }
        IEnumerable<Type> CommandTypes { get; }

        IEnumerable<Type> EventHandlerTypes { get; }
        IEnumerable<Type> EventTypes { get; }

        IEnumerable<Type> RequestHandlerTypes { get; }
        IEnumerable<Type> RequestTypes { get; }
    }
}