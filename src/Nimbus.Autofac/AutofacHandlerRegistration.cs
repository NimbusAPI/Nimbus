using System;
using Autofac;

namespace Nimbus.Autofac
{
    public class AutofacHandlerRegistration : IRegisterHandlers
    {
        private readonly ContainerBuilder _builder;

        public AutofacHandlerRegistration(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public void RegisterCommandHandlers(Type[] commandHandlerTypes)
        {
            _builder.RegisterTypes(commandHandlerTypes);
        }

        public void RegisterEventHandlers(Type[] eventHandlerTypes)
        {
            _builder.RegisterTypes(eventHandlerTypes);
        }

        public void RegisterRequestHandlers(Type[] requestHandlerTypes)
        {
            _builder.RegisterTypes(requestHandlerTypes);
        }
    }
}