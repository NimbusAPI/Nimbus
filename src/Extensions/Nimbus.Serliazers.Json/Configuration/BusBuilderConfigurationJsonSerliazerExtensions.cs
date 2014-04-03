using Newtonsoft.Json;

namespace Nimbus.Configuration
{
    /// <summary>
    ///     Extends <see cref="BusBuilderConfiguration" /> with support for the Json.Net serialization framework.
    /// </summary>
    public static class BusBuilderConfigurationJsonSerliazerExtensions
    {
        /// <summary>
        ///     To provide custom serialization <see cref="JsonSerializer" />.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serliazer to.</param>
        /// <param name="settings">To configure serialization settings.</param>
        /// <param name="formatting">To configure serialization formatting.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration, JsonSerializerSettings settings, Formatting formatting = Formatting.None)
        {
            return configuration
                .WithSerializer(new Nimbus.Serliazers.Json.JsonSerializer(settings, formatting));
        }

        /// <summary>
        ///     To provide custom serialization <see cref="JsonSerializer" />.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serliazer to.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration)
        {
            return configuration
                .WithSerializer(new Nimbus.Serliazers.Json.JsonSerializer());
        }
    }
}