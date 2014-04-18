using System.Collections.Generic;

namespace Nimbus.Configuration.Settings
{
    public abstract class Setting<T>: IValidatableConfigurationSetting
    {
        public T Value { get; set; }

        protected Setting()
        {
            Value = Default;
        }

        public static implicit operator T(Setting<T> setting)
        {
            return setting.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public IEnumerable<string> Validate()
        {
            yield break;
        }

        public virtual T Default
        {
            get { return default(T); }
        }
    }
}