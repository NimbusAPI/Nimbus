using System;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    internal static class FluentConfigurationExtensions
    {
        public static T Chain<T>(this T t, Func<T, T> apply)
        {
            return apply(t);
        }
    }
}