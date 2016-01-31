using Nimbus.Logger.Log4net;
using log4net;

namespace Nimbus.Configuration
{
    /// <summary>
    ///     Extends <see cref="BusBuilderConfiguration" /> with support for the Log4net logging framework.
    /// </summary>
    public static class BusBuilderConfigurationLog4NetExtensions
    {
        /// <summary>
        ///     Log to the provided Log4net <see cref="log4net.ILog" />.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the logger to.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithLog4netLogger(this BusBuilderConfiguration configuration, ILog logger)
        {
            return configuration
                .WithLogger(new Log4NetLogger(logger));
        }
    }
}