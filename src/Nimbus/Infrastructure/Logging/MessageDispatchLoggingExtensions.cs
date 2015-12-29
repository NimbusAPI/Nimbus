using System;
using System.Linq;
using Nimbus.Annotations;

namespace Nimbus.Infrastructure.Logging
{
    internal static class MessageLoggingExtensions
    {
        internal static void LogDispatchAction(this ILogger logger, string dispatchAction, string queueOrTopicPath, NimbusMessage message, TimeSpan elapsed)
        {
            var metadata = MessageMetadata.Create(message);
            logger.Debug("{DispatchAction} {ShortMessageTypeName} ({MessageId}) to {QueueOrTopicPath} ({@MessageMetadata}) ({Elapsed})",
                         dispatchAction,
                         metadata.ShortMessageTypeName,
                         metadata.MessageId,
                         queueOrTopicPath,
                         metadata,
                         elapsed);
        }

        internal static void LogDispatchError(this ILogger logger, string dispatchAction, string queueOrTopicPath, NimbusMessage message, TimeSpan elapsed, Exception exception)
        {
            var metadata = MessageMetadata.Create(message);
            logger.Error(exception,
                         "Error {DispatchAction} {ShortMessageTypeName} ({MessageId}) to {QueueOrTopicPath} ({@MessageMetadata}) ({Elapsed})",
                         dispatchAction,
                         metadata.ShortMessageTypeName,
                         metadata.MessageId,
                         queueOrTopicPath,
                         metadata,
                         elapsed);
        }

        private class MessageMetadata
        {
            public static MessageMetadata Create(NimbusMessage message)
            {
                var typeFullName = message.Payload.GetType().FullName;
                var shortMessageTypeName = typeFullName == null
                    ? null
                    : typeFullName.Split('.').Last();
                return new MessageMetadata(message.MessageId, message.CorrelationId, shortMessageTypeName, typeFullName);
            }

            private MessageMetadata(Guid messageId, Guid correlationId, string shortMessageTypeName, string messageType)
            {
                MessageId = messageId;
                CorrelationId = correlationId;
                ShortMessageTypeName = shortMessageTypeName;
                MessageType = messageType;
            }

            [UsedImplicitly]
            public Guid MessageId { get; }

            [UsedImplicitly]
            public Guid CorrelationId { get; }

            [UsedImplicitly]
            public string ShortMessageTypeName { get; }

            [UsedImplicitly]
            public string MessageType { get; }
        }
    }
}