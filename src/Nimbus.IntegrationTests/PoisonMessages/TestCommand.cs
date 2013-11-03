using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.PoisonMessages
{
    public class TestCommand : IBusCommand
    {
        public string SomeContent { get; set; }

        public TestCommand()
        {
        }

        public TestCommand(string someContent)
        {
            SomeContent = someContent;
        }
    }
}