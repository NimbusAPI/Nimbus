using System;
using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.WindowsServiceBus.Filtering
{
    internal static class MatchConditionSqlGenerator
    {
        public static string GenerateSqlFor(MatchCondition condition)
        {
            var valueRepresentation = ConvertToStringRepresentation(condition.RequiredValue);

            var filterExpression = $"{condition.PropertyKey} = {valueRepresentation}";
            return filterExpression;
        }

        private static string ConvertToStringRepresentation(object requiredValue)
        {
            if (requiredValue is string) return $"'{requiredValue}'";
            if (requiredValue is Guid) return $"'{requiredValue}'";
            return requiredValue.ToString();
        }
    }
}