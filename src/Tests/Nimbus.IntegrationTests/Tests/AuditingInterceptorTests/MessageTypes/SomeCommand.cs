using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes
{
    public class SomeCommand : IBusCommand
    {
        public int Foo { get; set; }

        public SomeCommand()
        {
        }

        public SomeCommand(int foo)
        {
            Foo = foo;
        }
    }
}