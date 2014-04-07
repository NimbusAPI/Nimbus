using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts
{
    public class BigFatResponse : IBusResponse
    {
        public string SomeBigAnswer { get; set; }
    }
}