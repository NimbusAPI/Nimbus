using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Dispatching;
using NSubstitute;

namespace Nimbus.UnitTests.NimbusMessageFactoryTests
{
    internal abstract class GivenANimbusMessageFactory : SpecificationForAsync<NimbusMessageFactory>
    {
        private IClock _clock;
        private ISerializer _serializer;
        protected ReplyQueueNameSetting ReplyQueueNameSetting;

        protected override async Task<NimbusMessageFactory> Given()
        {
            _clock = Substitute.For<IClock>();
            _serializer = Substitute.For<ISerializer>();

            ReplyQueueNameSetting = new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "TestApplication"},
                                                              new InstanceNameSetting {Value = "TestInstance"},
                                                              PathFactory.CreateWithNoPrefix());

            return new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                            ReplyQueueNameSetting,
                                            _clock,
                                            new DispatchContextManager()
                );
        }
    }
}