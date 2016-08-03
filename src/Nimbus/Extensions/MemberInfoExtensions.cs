using System;
using System.Linq;
using System.Reflection;

namespace Nimbus.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static bool HasAttribute<TAttr>(this MemberInfo memberInfo) where TAttr : Attribute
        {
            return memberInfo.GetCustomAttributes<TAttr>().Any();
        }
    }
}