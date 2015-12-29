using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
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
            var logger = TestHarnessLoggerFactory.Create();
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

            using (new AssertingStopwatch("Store", TimeSpan.FromSeconds(TimeoutSeconds)))
            {
                _storageKey = await Subject.Store(_id, _bytes, _expiresAfter);
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        [Test]
        public async Task TheRetrievedValueShouldBeTheSameAsTheStoredValue()
        {
            using (new AssertingStopwatch("Retrieve", TimeSpan.FromSeconds(TimeoutSeconds)))
            {
                var retrieved = await Subject.Retrieve(_storageKey);
                retrieved.ShouldBe(_bytes);
            }
        }

        public override void TearDown()
        {
            Console.WriteLine();
            Console.WriteLine();

            Subject.Delete(_storageKey).Wait();

            base.TearDown();
        }
    }
}