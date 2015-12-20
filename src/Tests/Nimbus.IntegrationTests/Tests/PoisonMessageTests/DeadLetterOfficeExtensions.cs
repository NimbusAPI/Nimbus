using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests
{
    internal static class DeadLetterOfficeExtensions
    {
        public static async Task<NimbusMessage[]> PopAll(this IDeadLetterOffice deadLetterOffice)
        {
            var messages = new List<NimbusMessage>();
            while (true)
            {
                var message = await deadLetterOffice.Pop();
                if (message == null) break;
                messages.Add(message);
            }

            return messages.ToArray();
        }
    }
}