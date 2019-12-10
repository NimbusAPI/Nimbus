using System;
using Microsoft.Extensions.Configuration;

public static class AppSettingsLoader {
    private static readonly Lazy<IConfiguration> _configuration = new Lazy<IConfiguration> (Build);

    public static TSetting Get<TSetting> (string key) {
        var value = (TSetting) _configuration.Value[key];
        return value;
    }

    private static IConfiguration Build () {
        var config = new ConfigurationBuilder ()
            .CreateDefaultBuilder()
            .AddEnvironmentVariables ("NIMBUS_")
            .Build ();

        return config;
    }
}