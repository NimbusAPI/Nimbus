namespace Nimbus.Configuration
{
    /// <summary>
    ///     Extends <see cref="BusBuilderConfiguration" /> with support for the NetJSON serialization framework.
    /// </summary>
    public static class BusBuilderConfigurationNetJsonSerializerExtensions
    {
        /// <summary>
        /// To provide custom serialization using the NetJSON serializer.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <param name="includeFields">if set to <c>true</c> fields are serialized, too.</param>
        /// <returns>
        /// Bus configuration.
        /// </returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration, bool includeFields)
        {
            return configuration.WithSerializer(new Nimbus.Serializers.NetJSON.NetJSONSerializer(includeFields));
        }

        /// <summary>
        /// To provide custom serialization using the NetJSON serializer.
        /// </summary>
        /// <param name="configuration">The bus configuration to apply the serializer to.</param>
        /// <returns>Bus configuration.</returns>
        public static BusBuilderConfiguration WithJsonSerializer(this BusBuilderConfiguration configuration)
        {
            return configuration.WithSerializer(new Nimbus.Serializers.NetJSON.NetJSONSerializer());
        }
    }
}