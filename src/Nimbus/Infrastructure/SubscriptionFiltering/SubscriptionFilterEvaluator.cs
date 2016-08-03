using Nimbus.Filtering;

namespace Nimbus.Infrastructure.SubscriptionFiltering
{
    internal static class SubscriptionFilterEvaluator
    {
        public static bool MatchesFilter(this NimbusMessage nimbusMessage, ISubscriptionFilter filter)
        {
            var isMatch = filter.FilterCondition.IsMatch(nimbusMessage.Properties);
            return isMatch;
        }
    }
}