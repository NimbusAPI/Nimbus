using System.Collections.Generic;

namespace Nimbus.InfrastructureContracts.Filtering.Conditions
{
    public interface IFilterCondition
    {
        bool IsMatch(IDictionary<string, object> messageProperties);
    }
}