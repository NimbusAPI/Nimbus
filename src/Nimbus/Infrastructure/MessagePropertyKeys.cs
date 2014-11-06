namespace Nimbus.Infrastructure
{
    public static class MessagePropertyKeys
    {
        public const string MessageId = "MessageId";
        public const string CorrelationId = "CorrelationId";
        public const string RequestTimeoutInMilliseconds = "RequestTimeoutInMilliseconds";
        public const string RequestSuccessful = "RequestSuccessful";
        public const string ExceptionMessage = "ExceptionMessage";
        public const string ExceptionType = "ExceptionType";
        public const string ExceptionStackTrace = "ExceptionStackTrace";
        public const string ExceptionTimestamp = "ExceptionTimestamp";
        public const string ExceptionMachineName = "ExceptionMachineName";
        public const string ExceptionIdentityName = "ExceptionIdentityName";
        public const string LargeBodyBlobIdentifier = "LargeBodyBlobIdentifier";
        public const string MessageType = "MessageType";
        public const string InReplyToRequestId = "InReplyToRequestId";
        public const string SentToQueue = "SentToQueue";
        public const string SentToTopic = "SentToTopic";
        public const string DispatchComplete = "DispatchComplete";
        public const string PrecedingMessageId = "PrecedingMessageId";
    }
}