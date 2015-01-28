using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.PropertyInjection;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.PropertyInjectionTests
{
    internal class WhenInjectingConfigurationSettingsIntoAHandler : SpecificationFor<PropertyInjector>
    {
        private object _handler;
        private IConfigurationSetting[] _settings;

        protected override PropertyInjector Given()
        {
            _settings = new IConfigurationSetting[]
                        {
                            new ApplicationNameSetting {Value = "TestApplicationName"},
                            new InstanceNameSetting {Value = "TestInstanceName"}
                        };
            return new PropertyInjector(Substitute.For<IClock>(),
                                        Substitute.For<IDispatchContextManager>(),
                                        Substitute.For<ILargeMessageBodyStore>(),
                                        Substitute.For<ILogger>(),
                                        _settings);
        }

        protected override void When()
        {
            _handler = new SomeHandler();
            Subject.Inject(_handler, new BrokeredMessage());
        }

        [Test]
        public void TheApplicationNameShouldBeCorrect()
        {
            ((IRequireSetting<ApplicationNameSetting>) _handler).Setting.Value.ShouldBe("TestApplicationName");
        }

        [Test]
        public void TheInstanceNameShouldBeCorrect()
        {
            ((IRequireSetting<InstanceNameSetting>) _handler).Setting.Value.ShouldBe("TestInstanceName");
        }

        public class SomeHandler : IRequireSetting<ApplicationNameSetting>, IRequireSetting<InstanceNameSetting>
        {
            ApplicationNameSetting IRequireSetting<ApplicationNameSetting>.Setting { get; set; }
            InstanceNameSetting IRequireSetting<InstanceNameSetting>.Setting { get; set; }
        }
    }
}