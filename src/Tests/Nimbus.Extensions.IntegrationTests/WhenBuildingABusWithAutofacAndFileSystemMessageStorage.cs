using System;
using System.IO;
using Autofac;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.LargeMessages.FileSystem.Configuration;
using Nimbus.Logger;
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
                                                  .WithNames("IntegrationTestHarness", Environment.MachineName)
                                                  .WithConnectionString(
                                                      @"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                                                  .WithLargeMessageStorage(sc => sc.WithLargeMessageBodyStore(c.Resolve<ILargeMessageBodyStore>())
                                                                                   .WithMaxSmallMessageSize(50*1024)
                                                                                   .WithMaxLargeMessageSize(1024*1024))
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