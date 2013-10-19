using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

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
                    var messages = ReceiveMessages();
                    var completedMessages = new List<BrokeredMessage>();
                    var abandonedMessages = new List<BrokeredMessage>();

                    foreach (var message in messages)
                    {
                        try
                        {
                            PumpMessage(message);
                            completedMessages.Add(message);
                        }
                        catch (Exception)
                        {
                            abandonedMessages.Add(message);
                            //FIXME log
                        }
                    }

                    completedMessages
                        .Select(m => m.CompleteAsync())
                        .WaitAll();

                    abandonedMessages
                        .Select(m => m.AbandonAsync())
                        .WaitAll();
                }
                catch (Exception exc)
                {
                    //FIXME log.
                }
            }
        }

        protected abstract BrokeredMessage[] ReceiveMessages();
        protected abstract void PumpMessage(BrokeredMessage message);
    }
}