using System.Collections.Generic;

namespace Nimbus.Tests.Unit
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings);
        }
    }
}