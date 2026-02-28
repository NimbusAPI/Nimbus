using Nimbus.Properties;
namespace Nimbus
{
    /// <summary>
    ///     We DELIBERATELY do not explicitly null-check any arguments in this codebase - it's too easy to forget. We
    ///     use Fody and apply it across the board. This is a test class (which needs to be in the core Nimbus assembly)
    ///     that allows our test suite to confirm that Fody is correctly injecting its NullGuard aspect.
    /// </summary>

    internal class FodyTests
    {
        // ReSharper disable once UnusedParameter.Local
        public FodyTests(object o)
        {
        }

        // ReSharper disable once UnusedParameter.Local
        public void DoFoo(object o)
        {
        }

        [UsedImplicitly]
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void DoBar(object o)
        {
        }
    }
}