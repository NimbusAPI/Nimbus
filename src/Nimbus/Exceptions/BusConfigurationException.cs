using System;
using System.Runtime.Serialization;

namespace Nimbus.Exceptions
{
    [Serializable]
    public class BusConfigurationException : BusException
    {
        public string ConfigurationSetting { get; protected set; }

        public BusConfigurationException(string configurationSetting, string message)
            : base(message)
        {
            ConfigurationSetting = configurationSetting;
        }

        protected BusConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}