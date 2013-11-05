using System;
using System.IO;

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
            return LocalFilesystemConnectionString() ?? FallbackConnectionString();
        }

        private static string LocalFilesystemConnectionString()
        {
            // this file can (and usually does) have passwords in it so it's important to have it NOT under source control anywhere
            const string filename = @"C:\Temp\NimbusConnectionString.txt";

            return !File.Exists(filename)
                       ? null
                       : File.ReadAllText(filename).Trim();
        }

        private static string FallbackConnectionString()
        {
            return @"Endpoint=sb://nimbustest.servicebus.windows.net/;SharedAccessKeyName=Demo;SharedAccessKey=bQppKwhg3xfBpIYqTAWcn9fC5HK1F2eh7G+AHb66jis=";
        }
    }
}