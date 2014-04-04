using System;
using System.IO;
using System.Runtime.Serialization;

namespace Nimbus.Serliazers
{
    public class DefaultSerializer : ISerializer
    {
        public Stream Serialize(object serializableObject)
        {
            var memoryStream = new MemoryStream();
            var serializer = new DataContractSerializer(serializableObject.GetType());
            serializer.WriteObject(memoryStream, serializableObject);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public object Deserialize(Stream stream, Type type)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var serializer = new DataContractSerializer(type);
            return serializer.ReadObject(stream);

        }
    }
}