using System;
using System.IO;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages
{
    internal class FileSystemLargeMessageBodyStore : ILargeMessageBodyStore
    {
        private readonly ILogger _logger;
        private readonly ThreadSafeLazy<DirectoryInfo> _storageDirectory;

        public FileSystemLargeMessageBodyStore(string storageDirectoryPath, ILogger logger)
        {
            _logger = logger;
            _storageDirectory = new ThreadSafeLazy<DirectoryInfo>(() => OpenStorageDirectory(storageDirectoryPath));
        }

        private DirectoryInfo OpenStorageDirectory(string storageDirectoryPath)
        {
            var directoryInfo = new DirectoryInfo(storageDirectoryPath);
            if (!directoryInfo.Exists) directoryInfo.Create();
            return directoryInfo;
        }

        public Task Store(string id, byte[] bytes, DateTimeOffset expiresAfter)
        {
            return Task.Run(() =>
                            {
                                var filename = ConstructFilename(id);
                                _logger.Debug("Writing blob {0} to {1}", id, filename);
                                File.WriteAllBytes(filename, bytes);
                            });
        }

        public Task<byte[]> Retrieve(string id)
        {
            return Task.Run(() =>
                            {
                                var filename = ConstructFilename(id);
                                _logger.Debug("Reading blob {0} from {1}", id, filename);
                                return File.ReadAllBytes(filename);
                            });
        }

        public Task Delete(string id)
        {
            return Task.Run(() =>
                            {
                                var filename = ConstructFilename(id);
                                _logger.Debug("Deleting blob {0} from {1}", id, filename);
                                File.Delete(filename);
                            });
        }

        private string ConstructFilename(string id)
        {
            return Path.Combine(_storageDirectory.Value.FullName, id);
        }
    }
}