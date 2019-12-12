using System.Collections;
using System.Collections.Generic;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    public class IoCContainerConfigurationSources : IEnumerable<IConfigurationScenario<ContainerConfiguration>>
    {
        public IEnumerator<IConfigurationScenario<ContainerConfiguration>> GetEnumerator()
        {
            yield return new NoContainerScenario();
            yield return new AutofacScenario();
            // yield return new NinjectScenario();
            // yield return new WindsorScenario();
            // yield return new UnityScenario();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}