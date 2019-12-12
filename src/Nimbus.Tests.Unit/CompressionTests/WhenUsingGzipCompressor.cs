using System.Text;
using Nimbus.Infrastructure.Compression;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.CompressionTests
{
    internal class WhenUsingGzipCompressor : SpecificationFor<GzipCompressor>
    {
        private readonly byte[] _uncompressed = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum convallis laoreet ligula, ut congue tellus condimentum eu. Duis mollis, nisi et adipiscing dapibus, leo lectus venenatis nibh, sed placerat enim mauris eget urna. Mauris et auctor augue. Morbi nec magna dapibus, molestie arcu id, venenatis eros. Ut a accumsan massa, quis suscipit neque. Quisque suscipit, lorem at malesuada luctus, odio urna commodo leo, vitae commodo lorem odio non nisi. Vivamus eros dui, aliquam eget nisi vitae, suscipit adipiscing lacus. Donec vel adipiscing dui. In et turpis vel metus ornare aliquam. Mauris placerat venenatis auctor. Suspendisse pretium lacus id dui congue, sed varius neque aliquet. Quisque cursus a velit id aliquet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. In pulvinar dignissim est a ultrices. Aliquam non ullamcorper risus.");
        private byte[] _compressed;
        private double _compressedLength;

        protected override GzipCompressor Given()
        {
            return new GzipCompressor();
        }

        protected override void When()
        {
            _compressed = Subject.Compress(_uncompressed);
            _compressedLength = (double)_compressed.Length;
        }

        [Test]
        public void ThenTheOutputShouldBeSmaller()
        {
            _compressedLength.ShouldBeLessThan((double)_uncompressed.Length);
        }

        [Test]
        public void ThenTheOutputShouldBeALotSmaller()
        {
            _compressedLength.ShouldBeLessThan(_uncompressed.Length/1.5);
        }
    }
}