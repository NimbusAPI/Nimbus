using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
    internal class SqlFilterExpressionGenerator : ISqlFilterExpressionGenerator
    {
        public string GenerateFor(IFilterCondition filterCondition)
        {
            return ConditionSqlGenerator.GenerateSqlFor(filterCondition);
        }

    }
}