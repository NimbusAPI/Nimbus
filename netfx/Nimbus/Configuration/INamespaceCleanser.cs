using System.Threading.Tasks;

namespace Nimbus.Configuration
{
    public interface INamespaceCleanser
    {
        /// <summary>
        ///     Danger! Danger, Will Robinson!
        /// </summary>
        Task RemoveAllExistingNamespaceElements();
    }
}