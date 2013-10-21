using System;

namespace Nimbus
{
    public interface IRegisterHandlers
    {
        void RegisterCommandHandlers(Type[] commandHandlerTypes);
        void RegisterEventHandlers(Type[] eventHandlerTypes);
        void RegisterRequestHandlers(Type[] requestHandlerTypes);
    }
}