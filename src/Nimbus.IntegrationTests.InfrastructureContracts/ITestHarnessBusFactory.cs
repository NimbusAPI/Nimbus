namespace Nimbus.IntegrationTests.InfrastructureContracts
{
    public interface ITestHarnessBusFactory
    {
        string BusFactoryName { get; }
        IBus Create();
    }
}