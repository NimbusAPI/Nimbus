using System;

namespace Nimbus.Infrastructure
{
    public interface ISerializer
    {
        string Serialize(object serializableObject);

        object Deserialize(string serializedObject, Type type);
    }
}