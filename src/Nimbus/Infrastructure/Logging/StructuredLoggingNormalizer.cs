using System.Linq;
using System.Text.RegularExpressions;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Logging
{
    public static class StructuredLoggingNormalizer
    {
        private static readonly Regex _regex = new Regex(@"{\S+?}", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        ///     Normalizes a log format string containing named placeholders (e.g. Message: {MessageId}) to a string containing
        ///     only
        ///     numeric placeholders (e.g. Message {0}).
        /// </summary>
        public static string Normalize(string format)
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