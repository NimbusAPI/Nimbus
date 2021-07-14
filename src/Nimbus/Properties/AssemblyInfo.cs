using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Nimbus.Tests.Common", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.Tests.Unit", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.Tests.Integration", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.StressTests", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.Containers.Autofac")]
[assembly: InternalsVisibleTo("Nimbus.Logger.Log4net")]
[assembly: InternalsVisibleTo("Nimbus.Logger.Serilog")]
[assembly: InternalsVisibleTo("Nimbus.Transports.InProcess")]
[assembly: InternalsVisibleTo("Nimbus.Transports.Redis")]
[assembly: InternalsVisibleTo("Nimbus.Transports.AzureServiceBus")]
[assembly: InternalsVisibleTo("Nimbus.Transports.Amqp")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2", AllInternalsVisible = true)]