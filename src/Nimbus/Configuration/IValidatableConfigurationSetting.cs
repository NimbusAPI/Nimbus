using System.Collections.Generic;

namespace Nimbus.Configuration
{
    public interface IValidatableConfigurationSetting
    {
        IEnumerable<string> Validate();
    }
}