using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.StartupPerformanceTests;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.LargeMessages.Azure.Infrastructure;
using Nimbus.Logger;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenPushingAndPullingDataFromAzureBlobStorage : SpecificationForAsync<AzureBlobStorageLargeMessageBodyStore>
    {
        private string _id;
        private DateTimeOffset _expiresAfter;
        private byte[] _bytes;
        private string _storageKey;

        protected override async Task<AzureBlobStorageLargeMessageBodyStore> Given()
        {
            var logger = new ConsoleLogger();
            return new AzureBlobStorageLargeMessageBodyStore(new BlobStorageConnectionStringSetting {Value = CommonResources.BlobStorageConnectionString}, logger);
        }

        protected override async Task When()
        {
            _id = Guid.NewGuid().ToString();
            _bytes = Encoding.UTF8.GetBytes(Enumerable.Range(0, 1024).Select(i => '.').ToArray());
            _expiresAfter = DateTimeOffset.UtcNow.AddDays(1);

            using (new AssertingStopwatch("Store", TimeSpan.FromSeconds(10)))
            {
                _storageKey = await Subject.Store(_id, _bytes, _expiresAfter);
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        [Test]
        public async Task TheRetrievedValueShouldBeTheSameAsTheStoredValue()
        {
            using (new AssertingStopwatch("Retrieve", TimeSpan.FromSeconds(10)))
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