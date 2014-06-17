using System;
using System.Linq;

namespace Nimbus.LargeMessages.Azure.Extensions
{
    internal static class UriExtensions
    {
        internal static Uri Append(this Uri uri, params string[] paths)
        {
            return new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
        }
    }
}