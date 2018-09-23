using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts
{
    public class BigFatRequest : IBusRequest<BigFatRequest, BigFatResponse>
    {
        public string SomeBigQuestion { get; set; }
    }
}