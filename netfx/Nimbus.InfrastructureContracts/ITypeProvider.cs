﻿using System;
using System.Collections.Generic;

namespace Nimbus
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
        IEnumerable<Type> ResponseTypes { get; }

        IEnumerable<Type> MulticastRequestHandlerTypes { get; }
        IEnumerable<Type> MulticastRequestTypes { get; }
        IEnumerable<Type> MulticastResponseTypes { get; }

        IEnumerable<Type> FilterTypes { get; }
        IEnumerable<Type> InterceptorTypes { get; }
    }
}