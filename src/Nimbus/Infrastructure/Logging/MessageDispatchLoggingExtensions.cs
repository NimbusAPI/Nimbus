using System;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Annotations;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Logging
{
    internal static class MessageLoggingExtensions
    {
        public static void LogDispatchAction(this ILogger logger, string dispatchAction, string queueOrTopicPath, BrokeredMessage message)
        {
            var metadata = MessageMetadata.Create(message);
            logger.Debug("{DispatchAction} {ShortMessageTypeName} ({MessageId}) to {QueueOrTopicPath} (@{MessageMetadata})",
                         dispatchAction,
                         metadata.ShortMessageTypeName,
                         metadata.MessageId,
                         queueOrTopicPath,
                         metadata);
        }

        public static void LogDispatchError(this ILogger logger, string dispatchAction, string queueOrTopicPath, BrokeredMessage message, Exception exception)
        {
            var metadata = MessageMetadata.Create(message);
            logger.Error(exception,
                         "Error {DispatchAction} {ShortMessageTypeName} ({MessageId}) to {QueueOrTopicPath} (@{MessageMetadata})",
                         dispatchAction,
                         metadata.ShortMessageTypeName,
                         metadata.MessageId,
                         queueOrTopicPath,
                         metadata);
        }

        private class MessageMetadata
        {
            private readonly Guid _messageId;
            private readonly Guid _correlationId;
            private readonly string _shortMessageTypeName;
            private readonly string _messageType;

            public static MessageMetadata Create(BrokeredMessage message)
            {
                var typeFullName = message.SafelyGetBodyTypeNameOrDefault();
                var typeName = typeFullName == null
                                   ? null
                                   : typeFullName.Split('.').Last();
                return new MessageMetadata(Guid.Parse(message.MessageId), Guid.Parse(message.CorrelationId), typeName, typeFullName);
            }

            private MessageMetadata(Guid messageId, Guid correlationId, string shortMessageTypeName, string messageType)
            {
                _messageId = messageId;
                _correlationId = correlationId;
                _shortMessageTypeName = shortMessageTypeName;
                _messageType = messageType;
            }

            [UsedImplicitly]
            public Guid MessageId
            {
                get { return _messageId; }
            }

            [UsedImplicitly]
            public Guid CorrelationId
            {
                get { return _correlationId; }
            }

            [UsedImplicitly]
            public string ShortMessageTypeName
            {
                get { return _shortMessageTypeName; }
            }

            [UsedImplicitly]
            public string MessageType
            {
                get { return _messageType; }
            }
        }
    }
}