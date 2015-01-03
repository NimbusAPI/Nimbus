using System;

namespace Nimbus.Serializers.NetJSON
{
    public class NetJSONSerializer : ISerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetJSONSerializer"/> class.
        /// </summary>
        public NetJSONSerializer()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetJSONSerializer" /> class.
        /// </summary>
        /// <param name="includeFields">if set to <c>true</c> fields are serialized, too.</param>
        public NetJSONSerializer(bool includeFields)
        {
            global::NetJSON.NetJSON.IncludeFields = includeFields;
        }

        public string Serialize(object serializableObject)
        {
            return global::NetJSON.NetJSON.Serialize(serializableObject);
        }

        public object Deserialize(string serializedObject, Type type)
        {
            return global::NetJSON.NetJSON.Deserialize(type, serializedObject);
        }
    }
}