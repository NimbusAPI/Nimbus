using System;

namespace Nimbus.Interceptors.Inbound
{
    public sealed class InterceptorAttribute : Attribute
    {
        private readonly Type _interceptorType;

        public InterceptorAttribute(Type interceptorType)
        {
            if (!typeof (IInboundInterceptor).IsAssignableFrom(interceptorType)) throw new ArgumentException("Type must be an interceptor type", "interceptorType");
            if (interceptorType.IsInterface) throw new ArgumentException("Interceptor type must be a concrete type", "interceptorType");

            _interceptorType = interceptorType;
        }

        public Type InterceptorType
        {
            get { return _interceptorType; }
        }

        public int? Priority { get; set; }
    }
}