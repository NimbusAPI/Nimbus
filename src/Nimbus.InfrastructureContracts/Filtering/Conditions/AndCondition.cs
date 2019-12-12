using System.Collections.Generic;
using System.Linq;

namespace Nimbus.InfrastructureContracts.Filtering.Conditions
{
    public class AndCondition : IFilterCondition
    {
        public IFilterCondition[] Conditions { get; }

        public AndCondition(params IFilterCondition[] conditions)
        {
            Conditions = conditions;
        }

        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            var isMatch = Conditions.All(c => c.IsMatch(messageProperties));
            return isMatch;
        }
    }
}