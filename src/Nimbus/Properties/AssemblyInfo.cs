using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("Nimbus.Tests.Common", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.UnitTests", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.IntegrationTests", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.StressTests", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Nimbus.Autofac")]
[assembly: InternalsVisibleTo("Nimbus.Windsor")]
[assembly: InternalsVisibleTo("Nimbus.LargeMessages.Azure")]
[assembly: InternalsVisibleTo("Nimbus.LargeMessages.FileSystem")]
[assembly: InternalsVisibleTo("Nimbus.Logger.Serilog")]
[assembly: InternalsVisibleTo("Nimbus.Transports.InProcess")]
[assembly: InternalsVisibleTo("Nimbus.Transports.Redis")]
[assembly: InternalsVisibleTo("Nimbus.Transports.AzureServiceBus")]
[assembly: InternalsVisibleTo("Nimbus.Transports.WindowsServiceBus")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2", AllInternalsVisible = true)]
