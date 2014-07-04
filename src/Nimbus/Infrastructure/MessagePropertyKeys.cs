namespace Nimbus.Infrastructure
{
    public static class MessagePropertyKeys
    {
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
    }
}