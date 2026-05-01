using Nimbus.Configuration;

namespace Nimbus.Transports.Nats
{
    public static class BusBuilderNatsConfigurationExtensions
    {
        public static BusBuilderConfiguration WithNatsTransport(this BusBuilderConfiguration configuration,
                                                                 string url = "nats://localhost:4222")
        {
            return configuration.WithTransport(new NatsTransportConfiguration().WithUrl(url));
        }
    }
}
