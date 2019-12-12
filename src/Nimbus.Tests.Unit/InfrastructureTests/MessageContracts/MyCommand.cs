// ReSharper disable once CheckNamespace

namespace Nimbus.Tests.Unit.InfrastructureTests.MessageContracts
{
    public class MyCommand<T>
    {
        public T Metadata { get; set; }
    }
}