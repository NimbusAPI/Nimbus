using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts
{
    public class GoBangEvent : IBusEvent
    {
        public string SomeContent { get; set; }

        public GoBangEvent()
        {
        }

        public GoBangEvent(string someContent)
        {
            SomeContent = someContent;
        }
    }
}