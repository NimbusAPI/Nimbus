namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

    internal interface ISqlFilterExpressionGenerator
    {
        string GenerateFor(IFilterCondition filterCondition);
    }
}