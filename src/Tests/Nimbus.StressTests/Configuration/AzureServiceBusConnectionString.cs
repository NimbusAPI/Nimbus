using System;
using ConfigInjector;

namespace Nimbus.StressTests.Configuration
{
    public class AzureServiceBusConnectionString : ConfigurationSetting<string>
    {
        public override string Value
        {
            get { return base.Value; }
            set { base.Value = value.Replace("{MachineName}", Environment.MachineName); }
        }
    }
}