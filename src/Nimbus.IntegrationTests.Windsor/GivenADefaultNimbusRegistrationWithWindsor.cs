using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shouldly;

namespace Nimbus.IntegrationTests.Windsor
{
    [TestFixture]
    public class GivenADefaultNimbusRegistrationWithWindsor : SpecificationFor<WindsorContainer>
    {
        public override WindsorContainer Given()
        {
            return new WindsorContainer();
        }

        public override void TearDown()
        {
 	        Subject.Dispose();
        }

        public override void When()
        {
            Subject.Register(Component.For<Nimbus.InfrastructureContracts.ILogger>().ImplementedBy<NullLogger>().LifestyleSingleton());
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());
            Subject.RegisterNimbus(typeProvider);

            Subject.Register(Component.For<IBus>().ImplementedBy<Bus>().UsingFactoryMethod<IBus>(
                () => new BusBuilder()
                    .Configure()
                    .WithConnectionString("Endpoint=sb://nimbusdemo.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=ouj58pTpzfbBu3hfSnX2fI57jzZgIBPuHsovjryCJCo=")
                    .WithNames("TestApp", "TestInstance")
                    .WithTypesFrom(typeProvider)
                    .WithWindsorDefaults(Subject)
                    .Build())
                    .LifestyleSingleton()
                    );
        }

        [Then]
        public void WeShouldBeAbleToResolveTheBus()
        {
            Subject.Resolve<IBus>().ShouldBeTypeOf<IBus>();
        }
    }
}
