using System;
using System.Collections.Generic;

namespace Nimbus.InfrastructureContracts
{
    public interface ITypeProvider
    {
        IEnumerable<Type> CommandHandlerTypes { get; }
        IEnumerable<Type> CommandTypes { get; }

        IEnumerable<Type> MulticastEventHandlerTypes { get; }
        IEnumerable<Type> CompetingEventHandlerTypes { get; }
        IEnumerable<Type> EventTypes { get; }

        IEnumerable<Type> RequestHandlerTypes { get; }
        IEnumerable<Type> RequestTypes { get; }

        void Verify();
    }
}