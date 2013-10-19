using System;
using System.Threading.Tasks;

namespace Nimbus.MessagePumps
{
    public abstract class MessagePump : IMessagePump
    {
        private bool _haveBeenToldToStop;

        public virtual void Start()
        {
            Task.Run((() => InternalMessagePump()));
        }

        public virtual void Stop()
        {
            _haveBeenToldToStop = true;
        }

        private void InternalMessagePump()
        {
            while (!_haveBeenToldToStop)
            {
                try
                {
                    PumpMessage();
                }
                catch (Exception exc)
                {
                    //FIXME log.
                }
            }
        }

        protected abstract void PumpMessage();
    }
}