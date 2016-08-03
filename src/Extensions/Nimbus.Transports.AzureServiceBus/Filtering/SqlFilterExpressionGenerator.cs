using System;
using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
    internal class SqlFilterExpressionGenerator : ISqlFilterExpressionGenerator
    {
        public string GenerateFor(IFilterCondition filterCondition)
        {
            //FIXME this is nowhere near done yet. It's a very first cut that does nothing but a single property match.
            var matchCondition = filterCondition as MatchCondition;
            if (matchCondition == null) throw new NotSupportedException("SQL expression filtering is not very well supported yet. A pull request here would be quite welcome :)");

            var valueRepresentation = ConvertToStringRepresentation(matchCondition.RequiredValue);

            var filterExpression = $"{matchCondition.PropertyKey} = {valueRepresentation}";
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