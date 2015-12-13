using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.DevelopmentStubs
{
    internal class StubDeadLetterOffice : IDeadLetterOffice
    {
        public async Task Post(NimbusMessage message)
        {
            throw new NotImplementedException();
        }
    }
}