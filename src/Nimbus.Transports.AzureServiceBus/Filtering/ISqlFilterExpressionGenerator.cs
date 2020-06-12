using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
    internal interface ISqlFilterExpressionGenerator
    {
        string GenerateFor(IFilterCondition filterCondition);
    }
}