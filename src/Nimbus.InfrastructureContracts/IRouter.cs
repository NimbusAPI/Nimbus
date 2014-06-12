using System;

namespace Nimbus
{
    public interface IRouter
    {
        string Route(Type messageType);
    }
}