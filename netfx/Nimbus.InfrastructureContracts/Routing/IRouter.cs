using System;

namespace Nimbus.Routing
{
    public interface IRouter
    {
        string Route(Type messageType, QueueOrTopic queueOrTopic, IPathFactory pathFactory);
    }
}