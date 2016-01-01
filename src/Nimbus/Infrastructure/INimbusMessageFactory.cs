using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface INimbusMessageFactory
    {
        /// <summary>
        ///     Ensures consistent construction of <see cref="NimbusMessage" />s
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>The newly minted <see cref="NimbusMessage" /></returns>
        Task<NimbusMessage> Create(string destinationPath, object payload);

        /// <summary>
        ///     Creates a <see cref="NimbusMessage" /> as successful response to the original request using the specified content
        ///     in the reply.
        /// </summary>
        /// <param name="responsePayload">The content to be serialized into the message body.</param>
        /// <param name="originalRequest">The original request this response is for.</param>
        /// <returns>The newly minted <see cref="NimbusMessage" /></returns>
        Task<NimbusMessage> CreateSuccessfulResponse(string destinationPath, object responsePayload, NimbusMessage originalRequest);

        /// <summary>
        ///     Creates a <see cref="NimbusMessage" /> as failure response to the original request with an empty body but the
        ///     right headers to indicate what went wrong.
        /// </summary>
        /// <param name="originalRequest">The original request this response is for.</param>
        /// <param name="exception">The exception that caused the request to fail.</param>
        /// <returns>The newly minted <see cref="NimbusMessage" /></returns>
        Task<NimbusMessage> CreateFailedResponse(string destinationPath, NimbusMessage originalRequest, Exception exception);
    }
}