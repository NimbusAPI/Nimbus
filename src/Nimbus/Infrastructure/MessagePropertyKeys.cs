using System;

namespace Nimbus.Infrastructure
{
    [Obsolete("We should be able to do away with this entire class if we put strongly-typed properties onto NimbusMessage.")]
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
        public const string SentToQueue = "SentToQueue";
        public const string SentToTopic = "SentToTopic";
        public const string RedeliveryToSubscriptionName = "RedeliveryToSubscriptionName";
    }
}