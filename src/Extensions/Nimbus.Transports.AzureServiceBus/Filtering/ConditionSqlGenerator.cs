using System;
using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
    internal static class ConditionSqlGenerator
    {
        public static string GenerateSqlFor(IFilterCondition condition)
        {
            if (condition is MatchCondition) return MatchConditionSqlGenerator.GenerateSqlFor((MatchCondition) condition);
            if (condition is AndCondition) return AndConditionSqlGenerator.GenerateSqlFor((AndCondition) condition);
            if (condition is OrCondition) return OrConditionSqlGenerator.GenerateSqlFor((OrCondition) condition);
            if (condition is NotCondition) return NotConditionSqlGenerator.GenerateSqlFor((NotCondition) condition);

            throw new NotSupportedException();
        }
    }
}