using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nimbus.InfrastructureContracts.Filtering.Conditions
{
    public class MatchCondition : IFilterCondition
    {
        public string PropertyKey { get; }
        public object RequiredValue { get; }

        public MatchCondition(string propertyKey, object requiredValue)
        {
            PropertyKey = propertyKey;
            RequiredValue = requiredValue;
        }

        public bool IsMatch(IDictionary<string, object> messageProperties)
        {
            object propertyValue;
            if (!messageProperties.TryGetValue(PropertyKey, out propertyValue)) return false;
            if (propertyValue == null) return false;

            var expectedType = RequiredValue.GetType();
            object actualValue;
            if (!ConvertToExpectedType(propertyValue, expectedType, out actualValue)) return false;

            var isMatch = Equals(RequiredValue, actualValue);
            return isMatch;
        }

        private bool ConvertToExpectedType(object propertyValue, Type expectedType, out object actualValue)
        {
            var actualType = propertyValue.GetType();
            if (actualType == expectedType)
            {
                actualValue = propertyValue;
                return true;
            }

            if (TryParse(propertyValue, expectedType, out actualValue)) return true;
            if (TryConvert(propertyValue, expectedType, out actualValue)) return true;

            return false;
        }

        private static bool TryParse(object propertyValue, Type expectedType, out object actualValue)
        {
            actualValue = null;
            var stringValue = propertyValue as string;
            if (stringValue == null) return false;

            var parseMethod = expectedType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                          .Where(m => m.Name == "Parse")
                                          .Where(m => m.GetParameters().Count() == 1)
                                          .Where(m => m.GetParameters()[0].ParameterType == typeof(string))
                                          .FirstOrDefault();

            if (parseMethod == null) return false;

            try
            {
                actualValue = parseMethod.Invoke(null, new object[] {stringValue});
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryConvert(object propertyValue, Type expectedType, out object actualValue)
        {
            try
            {
                actualValue = Convert.ChangeType(propertyValue, expectedType);
                return true;
            }
            catch (Exception)
            {
                actualValue = null;
                return false;
            }
        }
    }
}