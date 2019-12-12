using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Retries
{
    internal class DefaultRetry : Retry
    {
        private const int _numAttempts = 5;

        public DefaultRetry(ILogger logger) : base(_numAttempts)
        {
            this
                .Chain(r => r.Started += (s, e) => logger.Debug("{Action}...", e.ActionName))
                .Chain(r => r.Success += (s, e) => logger.Debug("{Action} completed successfully in {Elapsed}.", e.ActionName, e.ElapsedTime))
                .Chain(r => r.TransientFailure += (s, e) => logger.Warn(e.Exception, "A transient failure occurred in action {Action}.", e.ActionName))
                .Chain(r => r.PermanentFailure += (s, e) => logger.Error(e.Exception, "A permanent failure occurred in action {Action}.", e.ActionName))
                .WithBackoff<DefaultRetry>(Backoff)
                ;
        }

        private static async Task Backoff(RetryFailureEventArgs retryFailureEventArgs)
        {
            await Task.Delay(TimeSpan.FromSeconds(retryFailureEventArgs.FailedAttempts));
        }
    }
}