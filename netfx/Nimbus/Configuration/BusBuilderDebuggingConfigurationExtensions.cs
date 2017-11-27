﻿using Nimbus.Configuration.Debug;
using Nimbus.Configuration.Debug.Settings;
using Nimbus.Exceptions;

namespace Nimbus.Configuration
{
    public static class BusBuilderDebuggingConfigurationExtensions
    {
        private const string _confirmationMessage = "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites.";

        private static readonly string _requiresConfirmationMessage = string.Format(
            "You're asking me to do something dangerous but didn't provide the right confirmation message. You need to include exactly this string: '{0}'",
            _confirmationMessage);

        public static DebugConfiguration RemoveAllExistingNamespaceElementsOnStartup(this DebugConfiguration debuggingConfiguration)
        {
            throw new BusConfigurationException("RemoveAllExistingNamespaceElements", _requiresConfirmationMessage);
        }

        public static DebugConfiguration RemoveAllExistingNamespaceElementsOnStartup(this DebugConfiguration debuggingConfiguration, string confirmation)
        {
            if (confirmation != _confirmationMessage) throw new BusConfigurationException("RemoveAllExistingNamespaceElements", _requiresConfirmationMessage);

            debuggingConfiguration.RemoveAllExistingNamespaceElements = new RemoveAllExistingNamespaceElementsSetting {Value = true};
            return debuggingConfiguration;
        }
    }
}