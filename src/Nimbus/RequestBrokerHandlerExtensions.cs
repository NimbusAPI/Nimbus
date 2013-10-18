using System.Reflection;

namespace Nimbus
{
    public static class RequestBrokerHandlerExtensions
    {
        public static object HandleAwful(this IRequestBroker requestBroker, object request)
        {
            var handleMethod = ExtractHandlerMethodInfo(request);
            var response = handleMethod.Invoke(requestBroker, new object[] { request });
            return response;
        }

        public static MethodInfo ExtractHandlerMethodInfo(object request)
        {
            var genericHandleMethod = typeof (IRequestBroker).GetMethod("Handle");
            var requestGenericBaseType = request.GetType().BaseType;
            var genericArguments = requestGenericBaseType.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];
            var handleMethod = genericHandleMethod.MakeGenericMethod(new[] {requestType, responseType});
            return handleMethod;
        }
    }
}