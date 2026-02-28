namespace Nimbus.InfrastructureContracts.DependencyResolution
{
    public interface ICreateChildScopes
    {
        IDependencyResolverScope CreateChildScope();
    }
}