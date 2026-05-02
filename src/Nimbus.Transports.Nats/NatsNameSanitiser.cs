using System.Text.RegularExpressions;

namespace Nimbus.Transports.Nats
{
    internal static class NatsNameSanitiser
    {
        public static string Sanitise(string path)
        {
            var safe = Regex.Replace(path, @"[^a-zA-Z0-9_-]", "_");
            return safe.Length > 240 ? safe[..240] : safe;
        }
    }
}
