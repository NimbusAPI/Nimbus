using Jil;

namespace Nimbus.Configuration
{
    /// <summary>
    ///     Extends <see cref="BusBuilderConfiguration" /> with support for the Jil serialization framework.
    /// </summary>
    public static class BusBuilderConfigurationJilSerializerExtensions
    {
        /// <summary>
        /// To provide custom serialization using the Jil JSON serializer.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Bus configuration.
        /// </returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration, Options options)
        {
            return configuration.WithSerializer(new Nimbus.Serializers.Jil.JilSerializer(options));
        }

        /// <summary>
        /// To provide custom serialization using the Jil JSON serializer.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration)
        {
            return configuration.WithSerializer(new Nimbus.Serializers.Jil.JilSerializer());
        }
    }
}