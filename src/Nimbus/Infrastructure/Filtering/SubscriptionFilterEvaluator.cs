using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Infrastructure.Filtering
{
    internal static class SubscriptionFilterEvaluator
    {
        internal static bool MatchesFilter(this NimbusMessage nimbusMessage, IFilterCondition filterCondition)
        {
            var isMatch = filterCondition.IsMatch(nimbusMessage.Properties);
            return isMatch;
        }
    }
}