using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NSubstitute;

namespace Nimbus.IntegrationTests
{
    public class WhenPublishingAnEvent : SpecificationFor<Bus>
    {
        private NamespaceManager _namespaceManager;
        private MessagingFactory _messagingFactory;
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;

        public override Bus Given()
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(CommonResources.ConnectionString);
            _messagingFactory = MessagingFactory.CreateFromConnectionString(CommonResources.ConnectionString); 
            
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var bus = new Bus(_namespaceManager, _messagingFactory, _commandBroker, _requestBroker, _eventBroker, new[] { typeof(SomeCommand) }, new[] { typeof(SomeRequest) }, new [] {typeof(SomeEvent)});
            bus.Start();
            return bus;
        }

        public override void When()
        {

            var myEvent = new SomeEvent();
            Subject.Publish(myEvent);

            Thread.Sleep(2000);

            Subject.Stop();
        }


        [Then]
        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeEvent>());
        }

    }
}