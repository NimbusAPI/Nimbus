namespace Nimbus.DependencyResolution
{
    public interface ICreateChildScopes
    {
        IDependencyResolverScope CreateChildScope();
    }
}