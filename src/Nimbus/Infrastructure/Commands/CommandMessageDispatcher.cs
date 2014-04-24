using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.Interceptors;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly Type _commandType;
        private readonly IClock _clock;
        private readonly Type _handlerType;

        public CommandMessageDispatcher(
            IDependencyResolver dependencyResolver,
            IInboundInterceptorFactory inboundInterceptorFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            Type commandType,
            IClock clock,
            Type handlerType)
        {
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _commandType = commandType;
            _clock = clock;
            _handlerType = handlerType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busCommand = await _brokeredMessageFactory.GetBody(message, _commandType);
            await Dispatch((dynamic) busCommand, message);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, BrokeredMessage message) where TBusCommand : IBusCommand
        {
            try
            {
                using (var scope = _dependencyResolver.CreateChildScope())
                {
                    var handler = scope.Resolve<IHandleCommand<TBusCommand>>(_handlerType.FullName);
                    var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busCommand);

                    foreach (var interceptor in interceptors)
                    {
                        await interceptor.OnCommandHandlerExecuting(busCommand, message);
                    }

                    Exception exception;
                    try
                    {
                        var handlerTask = handler.Handle(busCommand);
                        var wrapper = new LongLivedTaskWrapper(handlerTask, handler as ILongRunningTask, message, _clock);
                        await wrapper.AwaitCompletion();

                        foreach (var interceptor in interceptors.Reverse())
                        {
                            await interceptor.OnCommandHandlerSuccess(busCommand, message);
                        }
                        return;
                    }
                    catch (Exception exc)
                    {
                        exception = exc;
                    }

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        await interceptor.OnCommandHandlerError(busCommand, message, exception);
                    }
                    throw exception;
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw;
            }
        }
    }
}