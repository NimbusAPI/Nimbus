using System;

namespace Nimbus.Filtering.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SubscriptionFilterAttribute : Attribute
    {
        public Type FilterType { get; }

        public SubscriptionFilterAttribute(Type filterType)
        {
            if (!typeof(ISubscriptionFilter).IsAssignableFrom(filterType)) throw new ArgumentException($"Filter types must implement {nameof(ISubscriptionFilter)}", nameof(filterType));

            FilterType = filterType;
        }
    }
}