using System;
using Autofac;
using Cafe.Messages;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json;
using Nimbus.Serializers.Json.Configuration;
using Nimbus.Transports.AzureServiceBus;
using Nimbus.Transports.Redis;
using Serilog;

namespace Waiter.Modules
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
                                                 //     new AzureServiceBusTransportConfiguration()
                                                 //         .WithConnectionString(Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTIONSTRING"))
                                                 //         .WithLargeMessageStorage(new AzureBlobStorageLargeMessageStorageConfiguration()
                                                 //                                  .UsingStorageAccountConnectionString(
                                                 //                                      Environment.GetEnvironmentVariable("AZURE_BLOB_STORE_CONNECTIONSTRING"))
                                                 //                                  .UsingBlobStorageContainerName(
                                                 //                                      Environment.GetEnvironmentVariable("AZURE_BLOB_STORE_CONTAINERNAME"))
                                                 //         )
                                                 // )
                                                 .WithTransport(new RedisTransportConfiguration().WithConnectionString("localhost"))
                                                 .WithNames("Waiter", Environment.MachineName)
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