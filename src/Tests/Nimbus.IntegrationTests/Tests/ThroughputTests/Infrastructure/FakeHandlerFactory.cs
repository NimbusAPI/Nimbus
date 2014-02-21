using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure
{
    public class FakeHandlerFactory : ICommandHandlerFactory, IMulticastEventHandlerFactory, ICompetingEventHandlerFactory, IRequestHandlerFactory, IMulticastRequestHandlerFactory
    {
        private readonly int _expectedNumMessagesReceived;
        private int _actualNumMessagesReceived;

        public FakeHandlerFactory(int expectedNumMessagesReceived)
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

        private void Dispatch<TBusCommand>(TBusCommand busEvent) where TBusCommand : IBusCommand
        {
            RecordMessageReceipt();
        }

        private void PublishMulticast<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            RecordMessageReceipt();
        }

        private void PublishCompeting<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            RecordMessageReceipt();
        }

        private TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        private IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
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

        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> ICompetingEventHandlerFactory.GetHandlers<TBusEvent>()
        {
            throw new NotImplementedException();
        }

        public OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent
        {
            throw new NotImplementedException();
        }

        public OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> GetHandlers<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        public OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            throw new NotImplementedException();
        }
    }
}