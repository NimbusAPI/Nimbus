using Newtonsoft.Json;
using JsonSerializer = Nimbus.Serializers.Json.JsonSerializer;

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
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration, JsonSerializerSettings settings)
        {
            return configuration.WithSerializer(new JsonSerializer(settings));
        }

        /// <summary>
        ///     To provide custom serialization using the <see cref="Newtonsoft.Json.JsonSerializer" />.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration)
        {
            return configuration.WithSerializer(new JsonSerializer());
        }
    }
}