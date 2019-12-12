using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Logging
{
    internal static class MessageLoggingExtensions
    {
        internal static void LogDispatchAction(this ILogger logger, string dispatchAction, string queueOrTopicPath, TimeSpan elapsed)
        {
            logger.Debug("{DispatchAction} message to {To} ({Elapsed})",
                         dispatchAction,
                         queueOrTopicPath,
                         elapsed);
        }

        internal static void LogDispatchError(this ILogger logger, string dispatchAction, string queueOrTopicPath, TimeSpan elapsed, Exception exception)
        {
            logger.Error(exception,
                         "Error {DispatchAction} message to {To} ({Elapsed})",
                         dispatchAction,
                         queueOrTopicPath,
                         elapsed);
        }
    }
}