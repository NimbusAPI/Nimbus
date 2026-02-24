using System;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration;

public enum TestTransport
{
    All,
    InProcess,
    Redis,
    Amqp,
    AzureServiceBus
}

public static class TransportSelector
{
    private static readonly Lazy<TestTransport> _selectedTransport = new(LoadFromEnvironment);

    public static TestTransport SelectedTransport => _selectedTransport.Value;

    private static TestTransport LoadFromEnvironment()
    {
        var envVar = Environment.GetEnvironmentVariable("NIMBUS_TEST_TRANSPORT") ?? "InProcess";

        if (Enum.TryParse<TestTransport>(envVar, ignoreCase: true, out var transport))
            return transport;

        return TestTransport.InProcess;
    }

    public static bool ShouldRunTransport(TestTransport transport)
    {
        if (SelectedTransport == TestTransport.All)
            return true;

        return SelectedTransport == transport;
    }


    public static bool ShouldRunTransport(string transportName)
    {
        return ShouldRunTransport(Enum.Parse<TestTransport>(transportName));
    }
}
