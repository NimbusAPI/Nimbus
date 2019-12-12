using System;
using Microsoft.Extensions.Configuration;

namespace Nimbus.Tests.Integration.Configuration
{
    public static class AppSettingsLoader {
        private static readonly Lazy<AppSettings> _configuration = new Lazy<AppSettings> (Build);

        public static AppSettings Settings => _configuration.Value;

        private static AppSettings Build () {
            var config = new ConfigurationBuilder ()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", true)
                .AddEnvironmentVariables ("NIMBUS_")
                .Build ();
        
            var appSettings = new AppSettings();
            config.Bind(appSettings);

            return appSettings;
        }
    }
}