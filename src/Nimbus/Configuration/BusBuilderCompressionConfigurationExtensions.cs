using Nimbus.Infrastructure.Compression;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    public static class BusBuilderCompressionConfigurationExtensions
    {
        public static BusBuilderConfiguration WithCompressor(this BusBuilderConfiguration configuration, ICompressor compressor)
        {
            configuration.Compressor = compressor;
            return configuration;
        }

        public static BusBuilderConfiguration WithDeflateCompressor(this BusBuilderConfiguration configuration)
        {
            return configuration.WithCompressor(new DeflateCompressor());
        }

        public static BusBuilderConfiguration WithGzipCompressor(this BusBuilderConfiguration configuration)
        {
            return configuration.WithCompressor(new GzipCompressor());
        }
    }
}