using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.WindowsServiceBus.Filtering
{
    internal class SqlFilterExpressionGenerator : ISqlFilterExpressionGenerator
    {
        public string GenerateFor(IFilterCondition filterCondition)
        {
            return ConditionSqlGenerator.GenerateSqlFor(filterCondition);
        }
    }
}