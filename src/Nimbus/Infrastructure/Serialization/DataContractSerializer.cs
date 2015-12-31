using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Serialization
{
    public class DataContractSerializer : ISerializer
    {
        private readonly DataContractSerializerSettings _settings;

        public DataContractSerializer(ITypeProvider typeProvider)
        {
            var dataContractResolver = new NimbusDataContractResolver(typeProvider);

            _settings = new DataContractSerializerSettings
                        {
                            DataContractResolver = dataContractResolver,
                            SerializeReadOnlyTypes = true,
                            MaxItemsInObjectGraph = int.MaxValue,
                            KnownTypes = typeProvider.AllSerializableTypes()
                        };
        }

        public string Serialize(object serializableObject)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new System.Runtime.Serialization.DataContractSerializer(serializableObject.GetType(), _settings);
                serializer.WriteObject(memoryStream, serializableObject);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memoryStream))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
        }

        public object Deserialize(string serializedObject, Type type)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedObject)))
            {
                var serializer = new System.Runtime.Serialization.DataContractSerializer(type, _settings);

                return serializer.ReadObject(memoryStream);
            }
        }
    }
}