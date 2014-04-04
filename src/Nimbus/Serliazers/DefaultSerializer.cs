using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Nimbus.Serliazers
{
    public class DefaultSerializer : ISerializer
    {
        public string Serialize(object serializableObject)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(serializableObject.GetType());
                serializer.WriteObject(memoryStream, serializableObject);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public object Deserialize(string serializedObject, Type type)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedObject)))
            {
                var serializer = new DataContractSerializer(type);
                return serializer.ReadObject(memoryStream);
            }
        }
    }
}