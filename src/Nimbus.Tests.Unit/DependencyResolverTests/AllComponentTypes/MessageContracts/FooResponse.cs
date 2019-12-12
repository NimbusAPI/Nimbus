using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.AllComponentTypes.MessageContracts
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