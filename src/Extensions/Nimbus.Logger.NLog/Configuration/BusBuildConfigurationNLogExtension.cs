using Nimbus.Logger.NLog;
using NLogger = NLog.Logger;

namespace Nimbus.Configuration
{
	public static class BusBuildConfigurationNLogExtension
	{
		/// <summary>
		///     Log to the provided NLog.
		/// </summary>
		/// <param name="configuration">The bus configuration to apply the logger to.</param>
		/// <param name="logger">The logger.</param>
		/// <returns>Bus configuration.</returns>
		public static BusBuilderConfiguration WithNLogLogger(this BusBuilderConfiguration configuration, NLogger logger)
		{
			return configuration
				.WithLogger(new NLogLogger(logger));
		}
	}
}
