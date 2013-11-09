using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts
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