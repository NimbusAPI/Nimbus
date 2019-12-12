using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DependencyResolverTests.DisposableComponents.MessageContracts;

namespace Nimbus.Tests.Unit.DependencyResolverTests.DisposableComponents.Handlers
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