namespace Nimbus.Extensions
{
    internal static class StringExtensions
    {
        internal static string FormatWith(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
    }
}