// using Microsoft.Practices.Unity;
// using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
// using Nimbus.Unity.Configuration;

// namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
// {
//     internal class UnityScenario : ConfigurationScenario<ContainerConfiguration>
//     {
//         public override ScenarioInstance<ContainerConfiguration> CreateInstance()
//         {
//             var unityContainer = new UnityContainer();
//             var configuration = new ContainerConfiguration
//                                 {
//                                     ApplyContainerDefaults = bbc =>
//                                                              {
//                                                                  unityContainer.RegisterNimbus(bbc.TypeProvider);

//                                                                  return bbc.WithUnityDependencyResolver(unityContainer);
//                                                              }
//                                 };
//             var instance = new ScenarioInstance<ContainerConfiguration>(configuration);
//             instance.Disposing += (s, e) => unityContainer.Dispose();

//             return instance;
//         }
//     }
// }