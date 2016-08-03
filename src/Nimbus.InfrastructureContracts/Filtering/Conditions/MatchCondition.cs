using System.Collections.Generic;

namespace Nimbus.Filtering.Conditions
{
    public class MatchCondition : IFilterCondition
    {
        public string PropertyKey { get; }
        public object PropertyValue { get; }

        public MatchCondition(string propertyKey, object propertyValue)
        {
            PropertyKey = propertyKey;
            PropertyValue = propertyValue;
        }

        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            object actualValue;
            if (!messageProperties.TryGetValue(PropertyKey, out actualValue)) return false;
            var isMatch = Equals(PropertyValue, actualValue);
            return isMatch;
        }
    }
}