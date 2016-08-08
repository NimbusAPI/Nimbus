using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.Stubs;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    internal class WhenPushingAndPullingDataFromAzureBlobStorage : SpecificationForAsync<AzureBlobStorageLargeMessageBodyStore>
    {
        private Guid _id;
        private DateTimeOffset _expiresAfter;
        private byte[] _bytes;
        private string _storageKey;

        protected override async Task<AzureBlobStorageLargeMessageBodyStore> Given()
        {
            var logger = TestHarnessLoggerFactory.Create(Guid.NewGuid(), GetType().FullName);
            return new AzureBlobStorageLargeMessageBodyStore(
                new AzureStorageAccountConnectionStringSetting {Value = DefaultSettingsReader.Get<AzureBlobStorageConnectionString>()},
                new AutoCreateBlobStorageContainerNameSetting(),
                logger);
        }

        protected override async Task When()
        {
            _id = Guid.NewGuid();
            _bytes = Encoding.UTF8.GetBytes(Enumerable.Range(0, 1024).Select(i => '.').ToArray());
            _expiresAfter = DateTimeOffset.UtcNow.AddDays(1);

            _storageKey = await Subject.Store(_id, _bytes, _expiresAfter);
        }

        [Test]
        public async Task TheRetrievedValueShouldBeTheSameAsTheStoredValue()
        {
            var retrieved = await Subject.Retrieve(_storageKey);
            retrieved.ShouldBe(_bytes);
        }

        public override void TearDown()
        {
            Subject.Delete(_storageKey).Wait();

            base.TearDown();
        }
    }
}