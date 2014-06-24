using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Outbound;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.InterceptorTests
{
    [TestFixture]
    public class WhenRegisteringInterceptorsWithAutofac
    {
        class DummyInterceptor : IOutboundInterceptor
        {
            public Task Decorate(BrokeredMessage brokeredMessage, object busMessage)
            {
                throw new NotImplementedException();
            }
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
                var interceptorSetting = new GlobalOutboundInterceptorTypesSetting {
                    Value = interceptorTypes
                };
                var outboundInterceptorFactory = new OutboundInterceptorFactory(interceptorSetting);
                var interceptors = outboundInterceptorFactory.CreateInterceptors(scope);
                Assert.AreEqual(1, interceptors.Count());
            }
        }
    }
}
