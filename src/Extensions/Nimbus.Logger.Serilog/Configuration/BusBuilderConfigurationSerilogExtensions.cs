using Nimbus.Logger.Serilog;
using ISerilogLogger = Serilog.ILogger;

namespace Nimbus.Configuration
{
    /// <summary>
    /// Extends <see cref="BusBuilderConfiguration"/> with support for the Serilog logging framework.
    /// </summary>
    public static class BusBuilderConfigurationSerilogExtensions
    {
        /// <summary>
        /// Log to the provided Serilog <see cref="Serilog.ILogger"/>.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the logger to.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithSerilogLogger(this BusBuilderConfiguration configuration, ISerilogLogger logger)
        {
            return configuration
                .WithLogger(new SerilogLogger(logger));
        }

        /// <summary>
        /// Log to the Serilog <see cref="Serilog.Log"/> class.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the logger to.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithSerilogLogger(this BusBuilderConfiguration configuration)
        {
            return configuration
                .WithLogger(new SerilogStaticLogger());
        }
    }
}