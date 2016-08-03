using Nimbus.Filtering.Conditions;

namespace Nimbus.Filtering
{
    public interface ISubscriptionFilter
    {
        IFilterCondition FilterCondition { get; }
    }
}