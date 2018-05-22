using Nimbus.MessageContracts;

namespace Nimbus.Testing.UnitTests.MessageContracts
{
    public class WaldoEvent : IBusEvent
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}