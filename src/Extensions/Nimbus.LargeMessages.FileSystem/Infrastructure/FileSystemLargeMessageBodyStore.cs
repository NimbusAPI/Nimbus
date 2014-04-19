using System;
using System.IO;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.FileSystem.Configuration.Settings;

namespace Nimbus.LargeMessages.FileSystem.Infrastructure
{
    internal class FileSystemLargeMessageBodyStore : ILargeMessageBodyStore
    {
        private readonly StorageDirectorySetting _storageDirectory;
        private readonly ILogger _logger;
        private readonly ThreadSafeLazy<DirectoryInfo> _directoryOnDisk;

        internal FileSystemLargeMessageBodyStore(StorageDirectorySetting storageDirectory, ILogger logger)
        {
            _storageDirectory = storageDirectory;
            _logger = logger;

            _directoryOnDisk = new ThreadSafeLazy<DirectoryInfo>(() => OpenStorageDirectory(_storageDirectory));
        }

        private DirectoryInfo OpenStorageDirectory(string storageDirectoryPath)
        {
            var directoryInfo = new DirectoryInfo(storageDirectoryPath);
            if (!directoryInfo.Exists) directoryInfo.Create();
            return directoryInfo;
        }

        public Task<string> Store(string id, byte[] bytes, DateTimeOffset expiresAfter)
        {
            return Task.Run(() =>
                            {
                                var storageKey = DefaultStorageKeyGenerator.GenerateStorageKey(id, expiresAfter);
                                var filename = ConstructFilename(storageKey);
                                EnsureDirectoryExistsFor(filename);
                                _logger.Debug("Writing blob {0} to {1}", id, filename);
                                File.WriteAllBytes(filename, bytes);
                                return storageKey;
                            });
        }

        private void EnsureDirectoryExistsFor(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var directoryInfo = fileInfo.Directory;
            if (!directoryInfo.Exists) directoryInfo.Create();
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
            return Path.Combine(_directoryOnDisk.Value.FullName, id);
        }
    }
}