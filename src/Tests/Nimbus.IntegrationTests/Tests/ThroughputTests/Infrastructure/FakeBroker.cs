using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure
{
    public class FakeBroker : ICommandHandlerFactory, IMulticastEventBroker, ICompetingEventBroker, IRequestBroker, IMulticastRequestBroker
    {
        private readonly int _expectedNumMessagesReceived;
        private int _actualNumMessagesReceived;

        public FakeBroker(int expectedNumMessagesReceived)
        {
            _expectedNumMessagesReceived = expectedNumMessagesReceived;
        }

        public int ActualNumMessagesReceived
        {
            get { return _actualNumMessagesReceived; }
        }

        public int ExpectedNumMessagesReceived
        {
            get { return _expectedNumMessagesReceived; }
        }

        public void WaitUntilDone(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (sw.Elapsed >= timeout) return;
                if (_actualNumMessagesReceived >= ExpectedNumMessagesReceived) return;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public void Dispatch<TBusCommand>(TBusCommand busEvent) where TBusCommand : IBusCommand
        {
            RecordMessageReceipt();
        }

        public void PublishMulticast<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            RecordMessageReceipt();
        }

        public void PublishCompeting<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            RecordMessageReceipt();
        }

        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        private void RecordMessageReceipt()
        {
            Interlocked.Increment(ref _actualNumMessagesReceived);

            if (_actualNumMessagesReceived%10 == 0)
            {
                Console.WriteLine("Seen {0} messages", _actualNumMessagesReceived);
            }
        }

        public OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand
        {
            throw new NotImplementedException();
        }
    }
}