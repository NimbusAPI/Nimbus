using System.Threading.Tasks;
using Nimbus.Configuration;

namespace Nimbus.Transports.InProcess.QueueManagement
{
    internal class NamespaceCleanser : INamespaceCleanser
    {
        private readonly InProcessMessageStore _messageStore;

        public NamespaceCleanser(InProcessMessageStore messageStore)
        {
            _messageStore = messageStore;
        }

        public Task RemoveAllExistingNamespaceElements()
        {
            return Task.Run(() => _messageStore.Clear());
        }
    }
}