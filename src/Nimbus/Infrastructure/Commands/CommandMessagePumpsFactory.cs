using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessagePumpsFactory : ICreateComponents
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ILogger _logger;
        private readonly CommandHandlerTypesSetting _commandHandlerTypes;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IClock _clock;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public CommandMessagePumpsFactory(ILogger logger,
                                          CommandHandlerTypesSetting commandHandlerTypes,
                                          INimbusMessagingFactory messagingFactory,
                                          IBrokeredMessageFactory brokeredMessageFactory,
                                          IClock clock,
                                          IDependencyResolver dependencyResolver)
        {
            _logger = logger;
            _commandHandlerTypes = commandHandlerTypes;
            _messagingFactory = messagingFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating command message pumps");

            foreach (var handlerType in _commandHandlerTypes.Value)
            {
                var commandTypes = handlerType.GetGenericInterfacesClosing(typeof (IHandleCommand<>)).Select(gi => gi.GetGenericArguments().First());

                foreach (var commandType in commandTypes)
                {
                    var queuePath = PathFactory.QueuePathFor(commandType);

                    _logger.Debug("Creating message pump for {0}", queuePath);

                    var messageReceiver = _messagingFactory.GetQueueReceiver(queuePath);

                    var dispatcher = new CommandMessageDispatcher(_dependencyResolver, _brokeredMessageFactory, commandType, _clock, handlerType);
                    _garbageMan.Add(dispatcher);

                    var pump = new MessagePump(messageReceiver, dispatcher, _logger, _clock);
                    _garbageMan.Add(pump);

                    yield return pump;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}