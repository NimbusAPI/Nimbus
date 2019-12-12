using Nimbus.MessageContracts;

namespace DevHarness.Messages
{
    public class TestCommand : IBusCommand
    {

        public int Id { get; set; }
        public string Name { get; set; }
        
    }
}