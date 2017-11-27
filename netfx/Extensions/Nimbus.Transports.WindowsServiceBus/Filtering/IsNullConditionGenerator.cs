using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.WindowsServiceBus.Filtering
{
    internal class IsNullConditionGenerator
    {
        public static string GenerateSqlFor(IsNullCondition condition)
        {
            return $"{condition.PropertyKey} IS NULL";
        }
    }
}