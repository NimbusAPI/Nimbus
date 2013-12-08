using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Nimbus.Extensions;

namespace Nimbus.Tests
{
    public static class MethodInfoExtensions
    {
        public static bool IsExtensionMethodFor<T>(this MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic) return false;
            if (!methodInfo.IsPublic) return false;
            if (!methodInfo.IsDefined(typeof (ExtensionAttribute), true)) return false;

            var args = methodInfo.GetParameters();
            if (args.None()) return false;

            var firstArg = args.First();
            return typeof (T).IsAssignableFrom(firstArg.ParameterType);
        }
    }
}