using Nimbus.DependencyResolution;
using Unity;

namespace Nimbus.Unity.Infastructure
{
    public class UnityDependencyResolver : IDependencyResolver
    {
        private readonly IUnityContainer _unityContainer;

        public UnityDependencyResolver(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new UnityDependencyResolverScope(_unityContainer.CreateChildContainer());
        }

        public void Dispose()
        {
            _unityContainer.Dispose();
        }
    }
}
