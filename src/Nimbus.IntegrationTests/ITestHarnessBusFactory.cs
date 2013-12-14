namespace Nimbus.IntegrationTests
{
    public interface ITestHarnessBusFactory
    {
        string BusFactoryName { get; }
        IBus Create();
    }
}