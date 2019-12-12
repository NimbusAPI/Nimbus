using System;
using Autofac;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json.Configuration;
using Nimbus.Transports.Amqp;
using Nimbus.Transports.Redis;

namespace DevHarness.Modules
{
    public class BusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            var handlerTypesProvider = new AssemblyScanningTypeProvider(typeof(BusModule).Assembly);

            //var transport = new RedisTransportConfiguration().WithConnectionString("localhost");
            var transport = new AmqpTransportConfiguration();
            
            builder.RegisterNimbus(handlerTypesProvider);
            builder.Register(componentContext => new BusBuilder()
                                                 .Configure()
                                                 .WithTransport(transport)
                                                 .WithNames("DevHarness", Environment.MachineName)
                                                 .WithTypesFrom(handlerTypesProvider)
                                                 .WithAutofacDefaults(componentContext)
                                                 .WithSerilogLogger()
                                                 .WithJsonSerializer()
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