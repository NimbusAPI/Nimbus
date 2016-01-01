namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition
{
    public interface IScenarioFilter
    {
        bool ShouldInclude(IConfigurationScenario scenario);
    }
}