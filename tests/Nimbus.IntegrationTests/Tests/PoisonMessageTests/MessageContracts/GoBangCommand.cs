using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts
{
    public class GoBangCommand : IBusCommand
    {
        public string SomeContent { get; set; }

        public GoBangCommand()
        {
        }

        public GoBangCommand(string someContent)
        {
            SomeContent = someContent;
        }
    }
}