using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using DataContractSerializer = Nimbus.Infrastructure.BrokeredMessageServices.Serialization.DataContractSerializer;

namespace Nimbus.UnitTests.CompressionTests
{
    internal class WhenBuildingBrokeredMessagesUsingCompression : SpecificationForAsync<BrokeredMessageFactory>
    {
        private BrokeredMessage _compressedMessage;
        private BrokeredMessage _uncompressedMessage;
        private BrokeredMessageFactory _defaultBrokeredMessageFactory;

        protected override async Task<BrokeredMessageFactory> Given()
        {
            _defaultBrokeredMessageFactory = BuildBrokeredMessageFactory(new NullCompressor());
            return BuildBrokeredMessageFactory(new DeflateCompressor());
        }

        private static BrokeredMessageFactory BuildBrokeredMessageFactory(ICompressor compressor)
        {
            return new BrokeredMessageFactory(
                new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "App"}, new InstanceNameSetting {Value = "Instance"}),
                new DataContractSerializer(),
                compressor,
                Substitute.For<IClock>(),
                new UnsupportedMessageBodyStore()
                );
        }

        protected override async Task When()
        {
            _uncompressedMessage = await _defaultBrokeredMessageFactory.Create(new CommandToCompress());
            _compressedMessage = await Subject.Create(new CommandToCompress());
        }

        [Test]
        public void ThenTheMessageShouldBeSmaller()
        {
            _compressedMessage.Size.ShouldBeLessThan(_uncompressedMessage.Size);
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