using System;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly IEventBroker _eventBroker;

        public Bus(IEventBroker eventBroker)
        {
            _eventBroker = eventBroker;
        }

        public void Send(object busCommand)
        {
            throw new NotImplementedException();
        }
    }
}