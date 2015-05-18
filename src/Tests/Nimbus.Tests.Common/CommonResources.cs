using System;
using System.IO;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;

namespace Nimbus.Tests.Common
{
    public class CommonResources
    {
        private static readonly ThreadSafeLazy<string> _connectionString = new ThreadSafeLazy<string>(FetchServiceBusConnectionString);
        private static readonly ThreadSafeLazy<string> _blobStorageConnectionString = new ThreadSafeLazy<string>(FetchBlobStorageConnectionString);

        public static string ServiceBusConnectionString
        {
            get { return _connectionString.Value; }
        }

        public static string BlobStorageConnectionString
        {
            get { return _blobStorageConnectionString.Value; }
        }

        private static string FetchServiceBusConnectionString()
        {
            return LocalFilesystemServiceBusConnectionString();
        }

        private static string LocalFilesystemServiceBusConnectionString()
        {
            // this file can (and usually does) have passwords in it so it's important to have it NOT under source control anywhere
			  const string envVariable = "NIMBUS_TESTS_ASB_CONNECTION_STRING";
			  const string filename = @"C:\Temp\NimbusConnectionString.txt";
			  string connectionString = null;

				// First try get the connection string from the env variable, then from the text file.
				connectionString = Environment.GetEnvironmentVariable(envVariable);
				if (string.IsNullOrEmpty(connectionString))
					if (File.Exists(filename))
						connectionString = File.ReadAllText(filename).Trim();

				if (string.IsNullOrEmpty(connectionString))
					throw new Exception("Could not find either the environment variable with name {0}, nor file {1} containing the Azure Service Bus connection string to use for integration testing".FormatWith(envVariable, filename));

				return connectionString;
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