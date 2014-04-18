using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.DependencyResolverTests.DisposableComponents.MessageContracts;

namespace Nimbus.UnitTests.DependencyResolverTests.DisposableComponents.Handlers
{
    public class DisposableHandler : IHandleCommand<NullCommand>, IDisposable
    {
        public DisposableHandler()
        {
            IsDisposed = false;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }

        public async Task Handle(NullCommand busCommand)
        {
        }
    }
}