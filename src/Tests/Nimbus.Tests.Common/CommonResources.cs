using System;
using System.IO;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;

namespace Nimbus.Tests.Common
{
    [Obsolete("These should be loaded via ConfigInjector")]
    public class CommonResources
    {
        private static readonly ThreadSafeLazy<string> _blobStorageConnectionString = new ThreadSafeLazy<string>(FetchBlobStorageConnectionString);

        public static string BlobStorageConnectionString
        {
            get { return _blobStorageConnectionString.Value; }
        }

        private static string FetchBlobStorageConnectionString()
        {
            const string filename = @"C:\Temp\NimbusBlobStorageConnectionString.txt";
            if (!File.Exists(filename))
                throw new Exception("Could not find the file {0} containing the Azure Blob Storage connection string to use for integration testing".FormatWith(filename));

            return File.ReadAllText(filename).Trim();
        }
    }
}