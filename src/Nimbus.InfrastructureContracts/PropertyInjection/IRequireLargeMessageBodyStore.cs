namespace Nimbus.PropertyInjection
{
    public interface IRequireLargeMessageBodyStore
    {
        ILargeMessageBodyStore LargeMessageBodyStore { get; set; }
    }
}