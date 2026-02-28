namespace Nimbus.InfrastructureContracts.PropertyInjection
{
    public interface IRequireDispatchContext
    {
        IDispatchContext DispatchContext { get; set; }
    }
}