using System;

namespace Nimbus.InfrastructureContracts.Routing
{
    public interface IRouter
    {
        string Route(Type messageType, QueueOrTopic queueOrTopic, IPathFactory pathFactory);
    }
}