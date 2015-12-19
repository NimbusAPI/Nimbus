namespace Nimbus.IntegrationTests.TestScenarioGeneration
{
    public abstract class PartialConfigurationScenario
    {
        public string Name { get; set; }

        protected PartialConfigurationScenario(string name)
        {
            Name = name;
        }

        public static string Combine(params string[] names)
        {
            return string.Join(".", names);
        }
    }

    public class PartialConfigurationScenario<T> : PartialConfigurationScenario
    {
        public PartialConfigurationScenario(string name, T configuration) : base(name)
        {
            Configuration = configuration;
        }

        public T Configuration { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}