using System;
using Newtonsoft.Json;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Serializers.Json
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        private static readonly JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
                                                                          {
                                                                              TypeNameHandling = TypeNameHandling.All,
                                                                              Formatting = Formatting.None,
                                                                              DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                              DateParseHandling = DateParseHandling.DateTimeOffset,
                                                                              DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                              ContractResolver = new CustomContractResolver()
                                                                          };

        public JsonSerializer()
            : this(_defaultSettings)
        {
        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        public string Serialize(object serializableObject)
        {
            return JsonConvert.SerializeObject(serializableObject, _settings);
        }

        public object Deserialize(string serializedObject, Type type)
        {
            return JsonConvert.DeserializeObject(serializedObject, type, _settings);
        }
    }
}