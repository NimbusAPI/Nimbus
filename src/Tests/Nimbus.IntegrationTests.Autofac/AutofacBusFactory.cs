using System;
using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.IntegrationTests.Autofac
{
    public class AutofacBusFactory : ITestHarnessBusFactory
    {
        private readonly ITypeProvider _typeProvider;
        private readonly string _connectionString;

        public AutofacBusFactory(ITypeProvider typeProvider, string connectionString)
        {
            _typeProvider = typeProvider;
            _connectionString = connectionString;
        }

        public string MessageBrokerName
        {
            get { return "AutofacMessageBrokers"; }
        }

        public IBus Create()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleLogger>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterNimbus(_typeProvider);
            builder.Register(componentContext => new BusBuilder()
                                 .Configure()
                                 .WithConnectionString(_connectionString)
                                 .WithNames("IntegrationTestHarness", Environment.MachineName)
                                 .WithTypesFrom(_typeProvider)
                                 .WithAutofacDefaults(componentContext)
                                 .WithDebugOptions(
                                     dc =>
                                         dc.RemoveAllExistingNamespaceElementsOnStartup(
                                             "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                 .Build())
                   .As<IBus>()
                   .AutoActivate()
                   .OnActivated(c => c.Instance.Start())
                   .SingleInstance();

            var container = builder.Build();
            var bus = container.Resolve<IBus>();
            return bus;
        }
    }
}