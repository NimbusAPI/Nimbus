using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.UnitTests.DispatcherTests.Handlers;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.DispatcherTests
{
    [TestFixture]
    internal class WhenDispatchingACommandToALongRunningHandler : SpecificationForAsync<LongLivedTaskWrapper>
    {
        private SlowCommand _slowCommand;
        private BrokeredMessage _brokeredMessage;
        private bool _renewLockCalled;
        private DateTimeOffset _lockedUntil;

        private SlowCommandHandler _handler;
        private Task _handlerTask;
        private SystemClock _clock;

        protected override async Task<LongLivedTaskWrapper> Given()
        {
            _slowCommand = new SlowCommand();
            _handler = new SlowCommandHandler();
            _brokeredMessage = new BrokeredMessage(_slowCommand);
            _clock = new SystemClock();

            _renewLockCalled = false;
            _handlerTask = _handler.Handle(_slowCommand);

            LongLivedTaskWrapperBase.RenewLockStrategy = m =>
                                                     {
                                                         _renewLockCalled = true;
                                                         _lockedUntil = _clock.UtcNow.AddSeconds(1);
                                                         _handler.PretendToBeWorkingSemaphore.Release();
                                                     };
            LongLivedTaskWrapperBase.LockedUntilUtcStrategy = m => _lockedUntil;

            _lockedUntil = DateTimeOffset.UtcNow.AddSeconds(1);
            return new LongLivedTaskWrapper(Substitute.For<ILogger>(), _handlerTask, _handler, _brokeredMessage, _clock);
        }

        protected override async Task When()
        {
            await Subject.AwaitCompletion();
        }

        [Test]
        public async Task RenewLockOnTheBrokeredMessageShouldBeCalled()
        {
            _renewLockCalled.ShouldBe(true);
        }
    }
}