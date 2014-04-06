using Newtonsoft.Json;

namespace Nimbus.Configuration
{
    /// <summary>
    ///     Extends <see cref="BusBuilderConfiguration" /> with support for the Json.Net serialization framework.
    /// </summary>
    public static class BusBuilderConfigurationJsonSerializerExtensions
    {
        /// <summary>
        ///     To provide custom serialization using the <see cref="Newtonsoft.Json.JsonSerializer" />.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <param name="settings">To configure serialization settings.</param>
        /// <param name="formatting">To configure serialization formatting.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration, JsonSerializerSettings settings, Formatting formatting = Formatting.None)
        {
            return configuration.WithSerializer(new Nimbus.Serializers.Json.JsonSerializer(settings, formatting));
        }

        /// <summary>
        ///     To provide custom serialization using the <see cref="Newtonsoft.Json.JsonSerializer" />.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration)
        {
            return configuration.WithSerializer(new Nimbus.Serializers.Json.JsonSerializer());
        }
    }
}