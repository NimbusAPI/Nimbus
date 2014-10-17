using Nimbus.Annotations;

namespace Nimbus
{
    /// <summary>
    ///     We DELIBERATELY do not explicitly null-check any arguments in this codebase - it's too easy to forget. We
    ///     use Fody and apply it across the board. This is a test class (which needs to be in the core Nimbus assembly)
    ///     that allows our test suite to confirm that Fody is correctly injecting its NullGuard aspect.
    /// </summary>
    public class FodyTests
    {
        public FodyTests(object o)
        {
        }

        public void DoFoo(object o)
        {
        }

        [UsedImplicitly]
        private void DoBar(object o)
        {
        }
    }
}