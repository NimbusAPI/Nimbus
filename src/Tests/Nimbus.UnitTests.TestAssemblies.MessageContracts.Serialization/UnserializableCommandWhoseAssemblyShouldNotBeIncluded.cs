using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization
{
    /// <summary>
    ///     We use this message type to test our AssemblyScanningTypeProvider this message type does not implementing a parameterless constractor
    ///     which makes this message type unserializalbe -gertjvr 1/3/2014
    /// </summary>
    public class UnserializableCommandWhoseAssemblyShouldNotBeIncluded : IBusCommand
    {
        private readonly string _a;

        public UnserializableCommandWhoseAssemblyShouldNotBeIncluded(string a)
        {
            _a = a;
        }
    }
}