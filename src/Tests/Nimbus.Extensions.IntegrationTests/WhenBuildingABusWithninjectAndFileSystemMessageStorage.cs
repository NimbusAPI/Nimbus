using System;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using Nimbus.Ninject.Configuration;
using Ninject;
using NUnit.Framework;

namespace Nimbus.Extensions.IntegrationTests
{
    using System.IO;

    using Nimbus.LargeMessages.FileSystem.Configuration;

    [TestFixture]
    public class WhenBuildingABusWithNinjectAndFileSystemMessageStorage
    {
        [Test]
        public void NothingShouldGoBang()
        {
            using (var container = new StandardKernel())
            {
                var typeProvider = new AssemblyScanningTypeProvider();

                container.Bind<ILogger>().To<ConsoleLogger>().InSingletonScope();

                container.RegisterNimbus(typeProvider);

                var largeMessageBodyTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Guid.NewGuid().ToString());

                container.Bind<ILargeMessageBodyStore>()
                         .ToMethod(
                             c =>
                             new FileSystemStorageBuilder().Configure()
                                                     .WithStorageDirectory(largeMessageBodyTempPath)
                                                     .WithLogger(c.Kernel.Get<ILogger>())
                                                     .Build())
                         .InSingletonScope();

                container.Bind<IBus>()
                         .ToMethod(
                             c =>
                             new BusBuilder().Configure()
                                             .WithNames("IntegrationTestHarness", Environment.MachineName)
                                             .WithConnectionString(
                                                 @"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                                             .WithLargeMessageStorage(
                                                 sc =>
                                                 sc.WithLargeMessageBodyStore(c.Kernel.Get<ILargeMessageBodyStore>())
                                                   .WithMaxSmallMessageSize(50 * 1024)
                                                   .WithMaxLargeMessageSize(1024 * 1024))
                                             .WithTypesFrom(typeProvider)
                                             .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                             .WithLogger(c.Kernel.Get<ILogger>())
                                             .Build())
                         .InSingletonScope();

                container.Get<IBus>();
            }
        }
    }
}