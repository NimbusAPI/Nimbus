namespace Nimbus.Configuration.Settings
{
    public class RequireRetriesToBeHandledBy : Setting<RetriesHandledBy>
    {
        public override RetriesHandledBy Default => RetriesHandledBy.Bus;
    }
}