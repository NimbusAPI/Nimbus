using System;
using System.IO;
using ProtoBuf;

namespace Nimbus.Serializers.ProtoBuf
{
    public class ProtoBufSerializer : ISerializer
    {
        public string Serialize(object serializableObject)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, serializableObject);
                return Convert.ToBase64String(
                    stream.GetBuffer(),
                    0,
                    (int) stream.Length);
            }
        }

        public object Deserialize(string serializedObject, Type type)
        {
            var bytes = Convert.FromBase64String(serializedObject);
            using (var stream = new MemoryStream(bytes))
            {

                var method = typeof (Serializer).GetMethod("Deserialize");
                method = method.MakeGenericMethod(type);

                return method.Invoke(this, new object[] {stream});
            }
        }
    }
}