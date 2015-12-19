using System.Threading.Tasks;
using Nimbus.Configuration;

namespace Nimbus.DevelopmentStubs
{
    internal class StubNamespaceCleanser: INamespaceCleanser
    {
        public async Task RemoveAllExistingNamespaceElements()
        {
        }
    }
}