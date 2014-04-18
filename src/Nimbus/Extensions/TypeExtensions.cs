using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Nimbus.Extensions
{
    internal static class TypeExtensions
    {
        internal static bool IsClosedTypeOf(this Type type, Type openGenericType)
        {
            if (!openGenericType.IsGenericType) throw new ArgumentException("It's a bit difficult to have a closed type of a non-open-generic type", "openGenericType");

            var interfaces = type.GetInterfaces();
            var baseTypes = new[] {type}.DepthFirst(t => t.BaseType == null ? new Type[0] : new[] {t.BaseType});
            var typeAndAllThatThatEntails = new[] {type}.Union(interfaces).Union(baseTypes).ToArray();
            var genericTypes = typeAndAllThatThatEntails.Where(i => i.IsGenericType);
            var closedGenericTypes = genericTypes.Where(i => !i.IsGenericTypeDefinition);
            var assignableGenericTypes = closedGenericTypes.Where(i => openGenericType.IsAssignableFrom(i.GetGenericTypeDefinition()));

            return assignableGenericTypes.Any();
        }

        internal static bool IsClosedTypeOf(this Type type, params Type[] openGenericTypes)
        {
            return openGenericTypes.Any(type.IsClosedTypeOf);
        }

        internal static bool IsInstantiable(this Type type)
        {
            if (type.IsInterface) return false;
            if (type.IsAbstract) return false;
            if (type.IsGenericType) return false;

            return true;
        }

        internal static Type[] GetGenericInterfacesClosing(this Type type, Type genericInterface)
        {
            var genericInterfaces = type.GetInterfaces()
                                        .Where(i => i.IsClosedTypeOf(genericInterface))
                                        .ToArray();
            return genericInterfaces;
        }

        internal static bool IsSerializable(this Type messageType)
        {
            try
            {
                using (var mem = new MemoryStream())
                {
                    var serializer = new DataContractSerializer(messageType);
                    var instance = Activator.CreateInstance(messageType, true);
                    serializer.WriteObject(mem, instance);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}