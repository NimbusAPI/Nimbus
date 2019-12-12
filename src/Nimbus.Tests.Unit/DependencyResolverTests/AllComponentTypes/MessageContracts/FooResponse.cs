using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.DependencyResolverTests.AllComponentTypes.MessageContracts
{
    public class FooResponse : IBusResponse
    {
        public string WhereDidIComeFrom { get; set; }

        public FooResponse()
        {
        }

        public FooResponse(string whereDidIComeFrom)
        {
            WhereDidIComeFrom = whereDidIComeFrom;
        }
    }
}