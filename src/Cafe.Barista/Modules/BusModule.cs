using System;
using Autofac;
using Cafe.Messages;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json.Configuration;
using Nimbus.Transports.AMQP;
using Nimbus.Transports.AzureServiceBus;
using Nimbus.Transports.Redis;
using Nimbus.Transports.SqlServer;

namespace Barista.Modules
{
    public class BusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var handlerTypesProvider = new AssemblyScanningTypeProvider(typeof(BusModule).Assembly, typeof(PlaceOrderCommand).Assembly);

            builder.RegisterNimbus(handlerTypesProvider);
            builder.Register(componentContext => new BusBuilder()
                                                 .Configure()
                                                 // .WithTransport(
                                                 //     new AzureServiceBusTransportConfiguration().WithConnectionString(Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTIONSTRING"))
                                                 //                                                .WithLargeMessageStorage(new AzureBlobStorageLargeMessageStorageConfiguration()
                                                 //                                                    .UsingStorageAccountConnectionString(Environment.GetEnvironmentVariable("AZURE_BLOB_STORE_CONNECTIONSTRING"))
                                                 //                                                    .UsingBlobStorageContainerName(Environment.GetEnvironmentVariable("AZURE_BLOB_STORE_CONTAINERNAME"))
                                                 //                                                )
                                                 //     )

                                                 // Redis Transport
                                                 //.WithTransport(new RedisTransportConfiguration().WithConnectionString("bus.iymtwr.0001.apse2.cache.amazonaws.com"))
                                                 //.WithTransport(new RedisTransportConfiguration().WithConnectionString("localhost"))

                                                 // ActiveMQ Transport
                                                 //.WithTransport(new AMQPTransportConfiguration()
                                                 //    .WithBrokerUri("amqp://localhost:5672")
                                                 //    .WithCredentials("admin", "admin"))

                                                 // SQL Server Transport
                                                 .WithTransport(new SqlServerTransportConfiguration()
                                                     .WithConnectionString("Server=localhost,1433;Database=Nimbus;User Id=sa;Password=Nimbus_Dev_123!;TrustServerCertificate=true;")
                                                     .WithAutoCreateSchema())

                                                 .WithNames("Barista", Environment.MachineName)
                                                 .WithTypesFrom(handlerTypesProvider)
                                                 .WithAutofacDefaults(componentContext)
                                                 .WithSerilogLogger()
                                                 .WithJsonSerializer()
//                                                 .WithGlobalInboundInterceptorTypes(typeof(CorrelationIdInterceptor))
                                                 .Build())
                   .As<IBus>()
                   .AutoActivate()
                   .OnActivated(c => c.Instance.Start())
                   .OnRelease(bus =>
                              {
                                  bus.Stop().Wait();
                                  bus.Dispose();
                              })
                   .SingleInstance();
        }
    }
}