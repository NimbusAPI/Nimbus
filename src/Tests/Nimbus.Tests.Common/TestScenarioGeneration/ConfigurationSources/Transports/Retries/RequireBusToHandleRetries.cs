using Nimbus.Configuration.Settings;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports.Retries
{
    public class RequireBusToHandleRetries : ConfigurationScenario<RequireRetriesToBeHandledBy>
    {
        public override ScenarioInstance<RequireRetriesToBeHandledBy> CreateInstance()
        {
            var instance = new RequireRetriesToBeHandledBy { Value = RetriesHandledBy.Bus };
            return new ScenarioInstance<RequireRetriesToBeHandledBy>(instance);
        }
    }
}