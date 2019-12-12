using System;

namespace Nimbus.InfrastructureContracts
{
    public interface ISerializer
    {
        string Serialize(object serializableObject);
        object Deserialize(string serializedObject, Type type);
    }
}