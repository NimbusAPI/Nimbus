using System;
using System.IO;
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


        public Stream Serialize(object serializableObject)
        {
            Newtonsoft.Json.JsonSerializer jsonSerializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
            jsonSerializer.Formatting = _formatting;

            var memoryStream = new MemoryStream();
            var sw = new StreamWriter(memoryStream);
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = jsonSerializer.Formatting;
                jsonSerializer.Serialize(jsonWriter, serializableObject);
            }

            return memoryStream;
        }

        public object Deserialize(Stream stream, Type type)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var jsonTextReader = new JsonTextReader(new StreamReader(stream));
            return serializer.Deserialize(jsonTextReader, type);
        }
    }
}