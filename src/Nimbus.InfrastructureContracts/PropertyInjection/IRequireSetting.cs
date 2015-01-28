namespace Nimbus.PropertyInjection
{
    public interface IRequireSetting<TSetting> where TSetting : IConfigurationSetting
    {
        TSetting Setting { get; set; }
    }
}