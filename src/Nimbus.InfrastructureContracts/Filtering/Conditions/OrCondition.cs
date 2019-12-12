using System.Collections.Generic;
using System.Linq;

namespace Nimbus.InfrastructureContracts.Filtering.Conditions
{
    public class OrCondition : IFilterCondition
    {
        public IFilterCondition[] Conditions { get; }

        public OrCondition(params IFilterCondition[] conditions)
        {
            Conditions = conditions;
        }

        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            var isMatch = Conditions.Any(c => c.IsMatch(messageProperties));
            return isMatch;
        }
    }
}