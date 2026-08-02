using System;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition
{
    /// <summary>
    ///     Declares that a fixture is expected to produce no test cases under some transport selections,
    ///     because what it covers genuinely doesn't apply there.
    ///     Without this, <see cref="TestCaseSources.AllBusConfigurations{T}" /> treats an empty scenario
    ///     set as a configuration error. A fixture that generates nothing is indistinguishable from one
    ///     that passed, so silence has to be opted into rather than being the default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AllowNoTestCasesAttribute : Attribute
    {
        public AllowNoTestCasesAttribute(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}
