using System;
using System.IO;
using Autofac;
using Nimbus.Configuration;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.LargeMessages.FileSystem.Configuration;
using Nimbus.Transports.WindowsServiceBus;
using NUnit.Framework;

namespace Nimbus.Extensions.IntegrationTests
{
    [TestFixture]
    public class WhenBuildingABusWithAutofacAndFileSystemMessageStorage
    {
        [Test]
        public void NothingShouldGoBang()
        {
            var builder = new ContainerBuilder();
            var typeProvider = new AssemblyScanningTypeProvider();

            builder.RegisterType<ConsoleLogger>()
                   .As<ILogger>()
                   .SingleInstance();

            builder.RegisterNimbus(typeProvider);

            var largeMessageBodyTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Guid.NewGuid().ToString());

            builder.Register(c => new FileSystemStorageBuilder().Configure()
                                                                .WithStorageDirectory(largeMessageBodyTempPath)
                                                                .WithLogger(c.Resolve<ILogger>())
                                                                .Build())
                   .As<ILargeMessageBodyStore>()
                   .SingleInstance();

            builder.Register(c => new BusBuilder().Configure()
                                                  .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                                     .WithConnectionString(
                                                                         @"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                                                                     .WithLargeMessageStorage(new LargeMessageStorageConfiguration()
                                                                                                  .WithMaxSmallMessageSize(50*1024)
                                                                                                  .WithMaxLargeMessageSize(1024*1024))
                                 )
                                                  .WithNames("IntegrationTestHarness", Environment.MachineName)
                                                  .WithTypesFrom(typeProvider)
                                                  .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                  .WithLogger(c.Resolve<ILogger>())
                                                  .Build())
                   .As<IBus>()
                   .SingleInstance();

            using (var container = builder.Build())
            {
                container.Resolve<IBus>();
            }
        }
    }
}