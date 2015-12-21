using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Castle.Windsor;
using Microsoft.Practices.Unity;
using Nimbus.Configuration;
using Nimbus.Ninject.Configuration;
using Nimbus.Unity.Configuration;
using Nimbus.Windsor.Configuration;
using Ninject;

namespace Nimbus.Tests.Common.TestScenarioGeneration
{
    public class IoCContainerConfigurationSources : IEnumerable<PartialConfigurationScenario<IoCContainerConfigurationSources.ContainerConfiguration>>
    {
        public IEnumerator<PartialConfigurationScenario<ContainerConfiguration>> GetEnumerator()
        {
            yield return new PartialConfigurationScenario<ContainerConfiguration>(
                "No container",
                new ContainerConfiguration
                {
                    ApplyContainerDefaults = bbc => bbc
                });

            yield return new PartialConfigurationScenario<ContainerConfiguration>(
                "Autofac",
                new ContainerConfiguration
                {
                    ApplyContainerDefaults = bbc =>
                                             {
                                                 var builder = new ContainerBuilder();
                                                 builder.RegisterNimbus(bbc.TypeProvider);
                                                 var container = builder.Build();

                                                 return bbc.WithAutofacDefaults(container);
                                             }
                });
            yield return new PartialConfigurationScenario<ContainerConfiguration>(
                "Ninject",
                new ContainerConfiguration
                {
                    ApplyContainerDefaults = bbc =>
                                             {
                                                 var kernel = new StandardKernel();
                                                 NimbusNinjectKernelExtensions.RegisterNimbus(kernel, bbc.TypeProvider);

                                                 return bbc.WithNinjectDefaults(kernel);
                                             }
                });
            yield return new PartialConfigurationScenario<ContainerConfiguration>(
                "Windsor",
                new ContainerConfiguration
                {
                    ApplyContainerDefaults = bbc =>
                                             {
                                                 var container = new WindsorContainer();
                                                 NimbusWindsorContainerExtensions.RegisterNimbus(container, bbc.TypeProvider);

                                                 return bbc.WithWindsorDefaults(container);
                                             }
                });
            yield return new PartialConfigurationScenario<ContainerConfiguration>(
                "Unity",
                new ContainerConfiguration
                {
                    ApplyContainerDefaults = bbc =>
                                             {
                                                 var container = new UnityContainer();
                                                 container.RegisterNimbus(bbc.TypeProvider);

                                                 return bbc.WithUnityDependencyResolver(container);
                                             }
                });
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class ContainerConfiguration
        {
            public Func<BusBuilderConfiguration, BusBuilderConfiguration> ApplyContainerDefaults { get; set; }
        }
    }
}