using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.WindowsServiceBus.Filtering
{
    internal static class NotConditionSqlGenerator
    {
        public static string GenerateSqlFor(NotCondition condition)
        {
            var innerExpression = ConditionSqlGenerator.GenerateSqlFor(condition.ConditionToNegate);
            var filterExpression = $"NOT ({innerExpression})";
            return filterExpression;
        }
    }
}