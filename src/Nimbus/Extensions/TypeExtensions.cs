using System;
using System.Linq;

namespace Nimbus.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsClosedTypeOf(this Type type, Type openGenericType)
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

        public static bool IsClosedTypeOf(this Type type, params Type[] openGenericTypes)
        {
            return openGenericTypes.Any(type.IsClosedTypeOf);
        }

        public static bool IsInstantiable(this Type type)
        {
            if (type.IsInterface) return false;
            if (type.IsAbstract) return false;
            if (type.IsGenericType) return false;

            return true;
        }

        public static Type[] GetGenericInterfacesClosing(this Type type, Type genericInterface)
        {
            var genericInterfaces = type.GetInterfaces()
                                        .Where(i => i.IsClosedTypeOf(genericInterface))
                                        .ToArray();
            return genericInterfaces;
        }
    }
}