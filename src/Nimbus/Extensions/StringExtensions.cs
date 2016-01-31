using System;
using Nimbus.Infrastructure.Logging;

namespace Nimbus.Extensions
{
    internal static class StringExtensions
    {
        internal static string FormatWith(this string s, params object[] args)
        {
            return String.Format(s, args);
        }

        internal static string NormalizeToStringFormat(this string format)
        {
            return StructuredLoggingNormalizer.Normalize(format);
        }
    }
}