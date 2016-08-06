using System.Collections.Generic;

namespace Nimbus.Filtering.Conditions
{
    public class NotCondition : IFilterCondition
    {
        public IFilterCondition ConditionToNegate { get; }

        public NotCondition(IFilterCondition conditionToNegate)
        {
            ConditionToNegate = conditionToNegate;
        }

        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            return !ConditionToNegate.IsMatch(messageProperties);
        }
    }
}