using System.IO;

namespace Nimbus.Serliazers
{
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this ISerializer serializer, Stream stream)
        {
            return (T)serializer.Deserialize(stream, typeof(T));
        }
    }
}