using System;

namespace Nimbus.Infrastructure
{
    internal interface IKnownMessageTypeVerifier
    {
        void AssertValidMessageType(Type messageType);
    }
}