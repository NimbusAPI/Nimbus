using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    internal static class TypeExtensions
    {
        public static bool IsTestClass(this Type type)
        {
            if (type.GetCustomAttributes<TestFixtureAttribute>().Any()) return true;
            if (type.GetMethods().Where(m => m.GetCustomAttributes<TestAttribute>().Any()).Any()) return true;

            return false;
        }
    }
}