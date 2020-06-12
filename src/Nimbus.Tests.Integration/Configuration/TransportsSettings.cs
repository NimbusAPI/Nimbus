namespace Nimbus.Tests.Integration.Configuration
{
    public class TransportsSettings
    {
        public RedisSettings Redis { get; set; }
        public AzureServiceBusSettings AzureServiceBus { get; set; }
    }
}