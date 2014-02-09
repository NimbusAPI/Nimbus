namespace Nimbus.Configuration.Settings
{
    public abstract class Setting<T>
    {
        public T Value { get; set; }

        public static implicit operator T(Setting<T> setting)
        {
            return setting.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}