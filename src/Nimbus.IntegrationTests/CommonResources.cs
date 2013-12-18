using System;
using System.IO;
using Nimbus.Extensions;

namespace Nimbus.IntegrationTests
{
    public class CommonResources
    {
        private static readonly Lazy<string> _connectionString = new Lazy<string>(FetchConnectionString);

        public static string ConnectionString
        {
            get { return _connectionString.Value; }
        }

        private static string FetchConnectionString()
        {
            return LocalFilesystemConnectionString();
        }

        private static string LocalFilesystemConnectionString()
        {
            // this file can (and usually does) have passwords in it so it's important to have it NOT under source control anywhere
            const string filename = @"C:\Temp\NimbusConnectionString.txt";

            if (!File.Exists(filename))
                throw new Exception("Could not find the file {0} containing the Azure Service Bus connection string to use for integration testing".FormatWith(filename));

            return File.ReadAllText(filename).Trim();
        }

    }
}