using System.Linq;
using System.Text.RegularExpressions;
using Nimbus.Extensions;

namespace Nimbus.Logging
{
    internal static class StructuredLoggingNormalizer
    {
        private static readonly Regex _regex = new Regex(@"{\S+?}", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static string NormalizeToStringFormat(this string format)
        {
            var output = format;

            var matchCollection = _regex.Matches(format);
            var matches = matchCollection
                                .Cast<Match>()
                                .DistinctBy(m => m.Captures[0].Value)
                                .ToArray();

            for (var i = 0; i < matches.Length; i++)
            {
                var match = matches[i];
                var replaceWith = string.Format("{{{0}}}", i);
                output = output.Replace(match.Captures[0].Value, replaceWith);
            }

            return output;
        }
    }
}