using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly ICommandSender _commandSender;
        private readonly IRequestSender _requestSender;
        private readonly IEventSender _eventSender;
        private readonly IMessagePump[] _messagePumps;

        public Bus(ICommandSender commandSender, IRequestSender requestSender, IEventSender eventSender, IEnumerable<IMessagePump> messagePumps)
        {
            _commandSender = commandSender;
            _requestSender = requestSender;
            _eventSender = eventSender;
            _messagePumps = messagePumps.ToArray();
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            await _commandSender.Send(busCommand);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var response = await _requestSender.SendRequest(busRequest);
            return response;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            await _eventSender.Publish(busEvent);
        }

        public void Start()
        {
            foreach (var pump in _messagePumps) pump.Start();
        }

        public void Stop()
        {
            foreach (var messagePump in _messagePumps) messagePump.Stop();
        }
    }
}