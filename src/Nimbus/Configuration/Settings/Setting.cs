namespace Nimbus.Configuration.Settings
{
    public abstract class Setting<T>
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

        public virtual T Default
        {
            get { return default(T); }
        }
    }
}