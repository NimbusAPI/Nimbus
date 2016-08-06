using Nimbus.Filtering.Conditions;

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