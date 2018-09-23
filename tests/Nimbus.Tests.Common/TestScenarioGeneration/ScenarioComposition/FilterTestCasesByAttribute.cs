using System;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FilterTestCasesByAttribute : Attribute
    {
        public Type Type { get; }

        public FilterTestCasesByAttribute(Type type)
        {
            Type = type;
        }
    }
}