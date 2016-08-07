// ReSharper disable once CheckNamespace

namespace Nimbus.UnitTests.InfrastructureTests.MessageContracts
{
    public class MyCommand<T>
    {
        public T Metadata { get; set; }
    }
}