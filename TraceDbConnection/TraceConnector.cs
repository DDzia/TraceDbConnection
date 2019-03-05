using System.Data.Common;
using Castle.DynamicProxy;
using TraceDbConnection.Interceptors;

namespace TraceDbConnection
{
    public static class TraceConnector
    {
        public static DbConnection Connect(DbConnection origin, ITraceReceiver traceReceiver)
        {
            var connectionInterceptor = new ConnectionInterceptor();
            connectionInterceptor.OnDiagnostic += traceReceiver.Save;

            var gen = new ProxyGenerator();
            return gen.CreateClassProxyWithTarget(origin, connectionInterceptor);
        }
    }
}
