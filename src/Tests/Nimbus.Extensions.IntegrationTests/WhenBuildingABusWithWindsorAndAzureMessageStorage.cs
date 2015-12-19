using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.LargeMessages.Azure.Configuration;
using Nimbus.Transports.WindowsServiceBus;
using Nimbus.Windsor.Configuration;
using NUnit.Framework;

namespace Nimbus.Extensions.IntegrationTests
{
    [TestFixture]
    public class WhenBuildingABusWithWindsorAndAzureMessageStorage
    {
        [Test]
        public void NothingShouldGoBang()
        {
            using (var container = new WindsorContainer())
            {
                var typeProvider = new AssemblyScanningTypeProvider();
                container.Register(Component.For<ILogger>()
                                            .ImplementedBy<ConsoleLogger>()
                                            .LifestyleSingleton());

                container.RegisterNimbus(typeProvider);

                container.Register(Component.For<IBus>()
                                            .UsingFactoryMethod(c => new BusBuilder().Configure()
                                                                                     .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                                                                        .WithConnectionString(
                                                                                                            @"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                                                                                                        .WithLargeMessageStorage(new AzureBlobStorageLargeMessageStorageConfiguration()
                                                                                                                                     .UsingStorageAccountConnectionString("Some fake connection string")
                                                                                                                                     .WithMaxSmallMessageSize(50*1024)
                                                                                                                                     .WithMaxLargeMessageSize(1024*1024))
                                                                    )
                                                                                     .WithNames("IntegrationTestHarness", Environment.MachineName)
                                                                                     .WithTypesFrom(typeProvider)
                                                                                     .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                                                     .WithLogger(c.Resolve<ILogger>())
                                                                                     .Build())
                                            .LifestyleSingleton());

                container.Resolve<IBus>();
            }
        }
    }
}