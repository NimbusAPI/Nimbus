using System.Collections.Generic;

namespace Nimbus.Configuration
{
    internal interface IValidatableConfigurationSetting
    {
        IEnumerable<string> Validate();
    }
}