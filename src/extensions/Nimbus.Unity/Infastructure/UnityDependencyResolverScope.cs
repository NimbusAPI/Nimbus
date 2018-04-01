using Nimbus.DependencyResolution;
using System;
using Unity;

namespace Nimbus.Unity.Infastructure
{
    public class UnityDependencyResolverScope : IDependencyResolverScope
    {
        private readonly IUnityContainer _unityContainer;

        public UnityDependencyResolverScope(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public TComponent Resolve<TComponent>()
        {
            return _unityContainer.Resolve<TComponent>();
        }

        public object Resolve(Type componentType)
        {
            return _unityContainer.Resolve(componentType);
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
