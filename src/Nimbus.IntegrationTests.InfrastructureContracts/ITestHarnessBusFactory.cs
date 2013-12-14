namespace Nimbus.IntegrationTests.InfrastructureContracts
{
    public interface ITestHarnessBusFactory
    {
        string MessageBrokerName { get; }
        IBus Create();
    }
}