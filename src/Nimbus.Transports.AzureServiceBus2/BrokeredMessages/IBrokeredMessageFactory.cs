namespace Nimbus.Transports.AzureServiceBus2.BrokeredMessages
{
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Nimbus.InfrastructureContracts;

    internal interface IBrokeredMessageFactory
    {
        Task<ServiceBusMessage> BuildMessage(NimbusMessage nimbusMessage);
        Task<NimbusMessage> BuildNimbusMessage(ServiceBusReceivedMessage message);
    }
}