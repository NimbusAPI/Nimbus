using System;
using System.Linq;
using System.Reflection;

namespace Nimbus.IntegrationTests.Extensions
{
    public static class MemberInfoExtensions
    {
        public static bool HasAttribute<TAttr>(this MemberInfo memberInfo) where TAttr : Attribute
        {
            return memberInfo.GetCustomAttributes<TAttr>().Any();
        }
    }
}