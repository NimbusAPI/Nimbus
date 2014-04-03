using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface IBrokeredMessageFactory
    {
        /// <summary>
        /// Ensures consistent construction of <see cref="BrokeredMessage"/>s
        /// </summary>
        /// <param name="serializableObject"></param>
        /// <returns>The newly minted <see cref="BrokeredMessage"/></returns>
        BrokeredMessage Create(object serializableObject);

        /// <summary>
        /// Creates a <see cref="BrokeredMessage"/> as successful response to the original request using the specified content in the reply.
        /// </summary>
        /// <param name="responseContent">The content to be serialized into the message body.</param>
        /// <param name="originalRequest">The original request this response is for.</param>
        /// <returns>The newly minted <see cref="BrokeredMessage"/></returns>
        BrokeredMessage CreateSuccessfulResponse(object responseContent, BrokeredMessage originalRequest);

        /// <summary>
        /// Creates a <see cref="BrokeredMessage"/> as failure response to the original request with an empty body but the right headers to indicate what went wrong.
        /// </summary>
        /// <param name="originalRequest">The original request this response is for.</param>
        /// <param name="exception">The exception that caused the request to fail.</param>
        /// <returns>The newly minted <see cref="BrokeredMessage"/></returns>
        BrokeredMessage CreateFailedResponse(BrokeredMessage originalRequest, Exception exception);
    }
}