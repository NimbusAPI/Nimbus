using System.Runtime.Serialization;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Compression;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.MessageContracts;
using Nimbus.Tests.Common.Stubs;
using NSubstitute;
using NUnit.Framework;
using DataContractSerializer = Nimbus.Infrastructure.Serialization.DataContractSerializer;

namespace Nimbus.UnitTests.CompressionTests
{
    internal class WhenBuildingNimbusMessagesUsingCompression : SpecificationForAsync<NimbusMessageFactory>
    {
        private NimbusMessage _compressedMessage;
        private NimbusMessage _uncompressedMessage;
        private NimbusMessageFactory _defaultNimbusMessageFactory;

        protected override async Task<NimbusMessageFactory> Given()
        {
            _defaultNimbusMessageFactory = BuildBrokeredMessageFactory(new NullCompressor());
            return BuildBrokeredMessageFactory(new DeflateCompressor());
        }

        private NimbusMessageFactory BuildBrokeredMessageFactory(ICompressor compressor)
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var serializer = new DataContractSerializer(typeProvider);
            return new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                            new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "App"}, new InstanceNameSetting {Value = "Instance"}, new PathFactory(new GlobalPrefixSetting())),
                                            Substitute.For<IClock>(),
                                            new DispatchContextManager());
        }

        protected override async Task When()
        {
            _uncompressedMessage = await _defaultNimbusMessageFactory.Create("nullQueue", new CommandToCompress());
            _compressedMessage = await Subject.Create("nullQueue", new CommandToCompress());
        }

        [Test]
        [Ignore("Not quite sure where these should fit yet.")]
        public void ThenTheMessageShouldBeSmaller()
        {
            //_compressedMessage.Size.ShouldBeLessThan(_uncompressedMessage.Size);
            Assert.Fail();
        }

        [DataContract]
        public class CommandToCompress : IBusCommand
        {
            public CommandToCompress()
            {
                Message =
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum convallis laoreet ligula, ut congue tellus condimentum eu. Duis mollis, nisi et adipiscing dapibus, leo lectus venenatis nibh, sed placerat enim mauris eget urna. Mauris et auctor augue. Morbi nec magna dapibus, molestie arcu id, venenatis eros. Ut a accumsan massa, quis suscipit neque. Quisque suscipit, lorem at malesuada luctus, odio urna commodo leo, vitae commodo lorem odio non nisi. Vivamus eros dui, aliquam eget nisi vitae, suscipit adipiscing lacus. Donec vel adipiscing dui. In et turpis vel metus ornare aliquam. Mauris placerat venenatis auctor. Suspendisse pretium lacus id dui congue, sed varius neque aliquet. Quisque cursus a velit id aliquet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. In pulvinar dignissim est a ultrices. Aliquam non ullamcorper risus.";
            }

            [DataMember]
            public string Message { get; set; }
        }
    }
}