using System.Collections.Generic;

namespace Nimbus.InfrastructureContracts.Filtering.Conditions
{
    public class IsNullCondition : IFilterCondition
    {
        public string PropertyKey { get; }

        public IsNullCondition(string propertyKey)
        {
            PropertyKey = propertyKey;
        }

        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            if (!messageProperties.ContainsKey(PropertyKey)) return true;
            if (messageProperties[PropertyKey] == null) return true;
            return false;
        }
    }
}