using System.Linq;
using Autofac;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
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
                var outboundInterceptorFactory = new OutboundInterceptorFactory(interceptorSetting);
                var interceptors = outboundInterceptorFactory.CreateInterceptors(scope);

                interceptors.Count().ShouldBe(1);
            }
        }
    }
}