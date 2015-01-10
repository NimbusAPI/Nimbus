using System;
using Nimbus.Extensions;
using Nimbus.MessageContracts;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Routing
{
    public class SingleQueueAndTopicPerMessageTypeRouter : IRouter
    {
        private readonly IRouter _fallbackRouter;

        private readonly string _singleCommandQueuePath;
        private readonly string _singleRequestQueuePath;

        public SingleQueueAndTopicPerMessageTypeRouter(IRouter fallbackRouter,
                                                       string singleCommandQueuePath = null,
                                                       string singleRequestQueuePath = null)
        {
            _fallbackRouter = fallbackRouter;
            _singleCommandQueuePath = singleCommandQueuePath;
            _singleRequestQueuePath = singleRequestQueuePath;
        }

        public string Route(Type messageType, QueueOrTopic queueOrTopic, IPathGenerator pathGenerator)
        {
            switch (queueOrTopic)
            {
                case QueueOrTopic.Queue:
                    if (typeof (IBusCommand).IsAssignableFrom(messageType))
                    {
                        return _singleCommandQueuePath ?? _fallbackRouter.Route(messageType, queueOrTopic, pathGenerator);
                    }

                    if (messageType.IsClosedTypeOf(typeof (IBusRequest<,>)))
                    {
                        return _singleRequestQueuePath ?? _fallbackRouter.Route(messageType, queueOrTopic, pathGenerator);
                    }

                    break;
                case QueueOrTopic.Topic:
                    break;
            }

            return _fallbackRouter.Route(messageType, queueOrTopic, pathGenerator);
        }
    }
}