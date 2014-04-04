using System;
using Newtonsoft.Json;

namespace Nimbus.Serializers.Json
{
    public class JsonSerializer : ISerializer
    {
        private readonly Formatting _formatting;
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer()
            : this(null, Formatting.None)
        {
        }

        public JsonSerializer(JsonSerializerSettings settings, Formatting formatting)
        {
            _settings = settings;
            _formatting = formatting;
        }

        public string Serialize(object serializableObject)
        {
            return JsonConvert.SerializeObject(serializableObject, _formatting, _settings);
        }

        public object Deserialize(string serializedObject, Type type)
        {
            return JsonConvert.DeserializeObject(serializedObject, type, _settings);
        }
    }
}