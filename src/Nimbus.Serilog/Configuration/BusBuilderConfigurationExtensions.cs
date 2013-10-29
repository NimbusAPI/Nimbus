using System;
using Nimbus.Serilog;
using Serilog;

namespace Nimbus.Configuration
{
    public static class BusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithSeriLogger(this BusBuilderConfiguration configuration, ILogger logger)
        {
            configuration.Logger = new SeriLogWrapper(logger);
            return configuration;
        }
    }
}