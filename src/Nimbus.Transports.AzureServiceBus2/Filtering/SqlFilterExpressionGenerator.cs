namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

    internal class SqlFilterExpressionGenerator : ISqlFilterExpressionGenerator
    {
        public string GenerateFor(IFilterCondition filterCondition)
        {
            return ConditionSqlGenerator.GenerateSqlFor(filterCondition);
        }

    }
}