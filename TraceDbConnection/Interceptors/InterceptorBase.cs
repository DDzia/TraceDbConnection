using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace TraceDbConnection.Interceptors
{
    abstract class InterceptorBase: IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var isAsyncMethod = returnType == typeof(Task) || returnType.IsSubclassOf(typeof(Task));

            if (isAsyncMethod)
            {
                InterceptAsync(invocation);
            }
            else
            {
                InterceptSync(invocation);
            }
        }

        public void InterceptSync(IInvocation invocation)
        {
            if (!ProxyCall(invocation))
            {
                invocation.Proceed();
            }
        }

        public void InterceptAsync(IInvocation invocation)
        {
            if (!ProxyCallAsync(invocation))
            {
                invocation.Proceed();
            }
        }

        protected virtual bool ProxyCall(IInvocation invocation)
        {
            return false;
        }

        protected virtual bool ProxyCallAsync(IInvocation invocation)
        {
            return false;
        }
    }
}
