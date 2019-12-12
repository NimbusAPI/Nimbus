namespace Nimbus.InfrastructureContracts.PropertyInjection
{
    public interface IRequireLargeMessageBodyStore
    {
        ILargeMessageBodyStore LargeMessageBodyStore { get; set; }
    }
}