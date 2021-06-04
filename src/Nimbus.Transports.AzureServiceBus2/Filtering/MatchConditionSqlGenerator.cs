namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using System;
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

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