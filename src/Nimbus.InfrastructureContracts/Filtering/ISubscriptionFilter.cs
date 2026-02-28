using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.InfrastructureContracts.Filtering
{
    public interface ISubscriptionFilter
    {
        IFilterCondition FilterCondition { get; }
    }
}