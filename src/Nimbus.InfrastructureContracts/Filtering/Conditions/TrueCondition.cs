using System.Collections.Generic;

namespace Nimbus.InfrastructureContracts.Filtering.Conditions
{
    public class TrueCondition: IFilterCondition
    {
        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            return true;
        }
    }
}