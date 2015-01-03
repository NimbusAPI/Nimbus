using System;
using Jil;

namespace Nimbus.Serializers.Jil
{
    public class JilSerializer : ISerializer
    {
        private readonly Options _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JilSerializer"/> class with the underlying
        /// <see cref="Options"/> set to <see cref="Options.ISO8601IncludeInherited"/>.
        /// </summary>
        public JilSerializer()
            : this(Options.ISO8601IncludeInherited)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JilSerializer"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public JilSerializer(Options options)
        {
            _options = options;
        }

        public string Serialize(object serializableObject)
        {
            // as object is used here rather than specific types which Jil could optimize for internally,
            // .SerializeDynamic(..) is the better fit here. See https://github.com/kevin-montrose/Jil#dynamic-deserialization
            return JSON.SerializeDynamic(serializableObject, _options);
        }

        public object Deserialize(string serializedObject, Type type)
        {
            // In contrast to Serialize above, a proper target type is provided and hence the better performing
            // .Deserialize() method can be used.
            return JSON.Deserialize(serializedObject, type, _options);
        }
    }
}