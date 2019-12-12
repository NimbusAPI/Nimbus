using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Tests.Integration.Tests.PoisonMessageTests
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

        public static async Task<NimbusMessage[]> PopAll(this IDeadLetterOffice deadLetterOffice, int numMessagesExpected, TimeSpan timeout)
        {
            var messages = new List<NimbusMessage>();

            while (true)
            {
                var message = await deadLetterOffice.Pop();
                if (message == null)
                {
                    if (messages.Count == numMessagesExpected) break;
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    continue;
                }

                messages.Add(message);
            }

            return messages.ToArray();
        }
    }
}