using System.Linq;
using Autofac;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Outbound;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.InterceptorTests
{
    [TestFixture]
    public class WhenRegisteringInterceptorsWithAutofac
    {
        private class DummyInterceptor : OutboundInterceptor
        {
        }

        [Test]
        public void TheyShouldBeResolvable()
        {
            var interceptorTypes = new[] {typeof (DummyInterceptor)};

            var builder = new ContainerBuilder();
            var typeProvider = Substitute.For<ITypeProvider>();
            typeProvider.InterceptorTypes.Returns(interceptorTypes);

            builder.RegisterNimbus(typeProvider);

            using (var container = builder.Build())
            using (var dependencyResolver = container.Resolve<IDependencyResolver>())
            using (var scope = dependencyResolver.CreateChildScope())
            {
                var interceptorSetting = new GlobalOutboundInterceptorTypesSetting
                                         {
                                             Value = interceptorTypes
                                         };
                var outboundInterceptorFactory = new OutboundInterceptorFactory(interceptorSetting,
                                                                                new PropertyInjector(Substitute.For<IClock>(),
                                                                                                     Substitute.For<IDispatchContextManager>(),
                                                                                                     Substitute.For<ILargeMessageBodyStore>(),
                                                                                                     Substitute.For<ILogger>(),
                                                                                                     new IConfigurationSetting[0]));

                var dummyBrokeredMessage = new BrokeredMessage();
                var interceptors = outboundInterceptorFactory.CreateInterceptors(scope, dummyBrokeredMessage);

                interceptors.Count().ShouldBe(1);
            }
        }
    }
}