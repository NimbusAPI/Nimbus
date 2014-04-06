using System;

namespace Nimbus
{
    public interface ISerializer
    {
        string Serialize(object serializableObject);

        object Deserialize(string serializedObject, Type type);
    }
}