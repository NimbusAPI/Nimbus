using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.IntegrationTests.Windsor
{
    public class WindsorBusFactory : ITestHarnessBusFactory
    {
        private readonly ITypeProvider _typeProvider;
        private readonly string _connectionString;

        public WindsorBusFactory(ITypeProvider typeProvider, string connectionString)
        {
            _typeProvider = typeProvider;
            _connectionString = connectionString;
        }

        public string MessageBrokerName
        {
            get { return "WindsorMessageBrokers"; }
        }

        public IBus Create()
        {
            var container = new WindsorContainer();
            container.Register(Component.For<ILogger>().ImplementedBy<ConsoleLogger>().LifestyleSingleton());
            container.RegisterNimbus(_typeProvider);

            container.Register(Component
                                   .For<IBus>()
                                   .ImplementedBy<Bus>()
                                   .UsingFactoryMethod<IBus>(() => new BusBuilder()
                                                                 .Configure()
                                                                 .WithConnectionString(_connectionString)
                                                                 .WithNames("TestApp", "TestInstance")
                                                                 .WithTypesFrom(_typeProvider)
                                                                 .WithWindsorDefaults(container)
                                                                 .WithDebugOptions(
                                                                     dc =>
                                                                         dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                                             "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                                                 .Build())
                                   .OnCreate(b => ((Bus) b).Start())
                                   .LifestyleSingleton()
                );

            return container.Resolve<IBus>();
        }
    }
}