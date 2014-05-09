using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nimbus.Extensions
{
    internal static class TaskExtensions
    {
        internal static void WaitAll(this IEnumerable<Task> tasks)
        {
            Task.WaitAll(tasks.ToArray());
        }

        internal static async Task WhenAll(this IEnumerable<Task> tasks)
        {
            await Task.WhenAll(tasks);
        }
    }
}