using System;
using System.IO;

namespace Nimbus
{
    public interface ISerializer
    {
        Stream Serialize(object serializableObject);

        object Deserialize(Stream stream, Type type);
    }
}