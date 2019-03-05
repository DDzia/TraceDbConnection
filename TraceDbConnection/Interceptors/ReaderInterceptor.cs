using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace TraceDbConnection.Interceptors
{
    class ReaderInterceptor: InterceptorBase
    {
        public event Action OnSwitchToDataset;
        public event Action OnSwitchFromDataset;

        public event Action OnDatasetEndReaded;

        public event Action OnStartReadRow;
        public event Action OnEndReadRow;

        protected override bool ProxyCall(IInvocation invocation)
        {
            if (invocation.Method.Name == nameof(DbDataReader.Read))
            {
                ProxyRead(invocation);
                return true;
            }

            if (invocation.Method.Name == nameof(DbDataReader.NextResult))
            {
                ProxyNextResult(invocation);
                return true;
            }

            return base.ProxyCall(invocation);
        }

        protected override bool ProxyCallAsync(IInvocation invocation)
        {
            if (invocation.Method.Name == nameof(DbDataReader.ReadAsync))
            {
                ProxyReadAsync(invocation);
                return true;
            }

            if (invocation.Method.Name == nameof(DbDataReader.NextResultAsync))
            {
                ProxyNextResultAsync(invocation);
                return true;
            }

            return base.ProxyCallAsync(invocation);
        }

        public void ProxyRead(IInvocation invocation)
        {
            OnStartReadRow?.Invoke();
            invocation.Proceed();

            var endOfSet = !(bool)invocation.ReturnValue;
            if (endOfSet)
            {
                OnDatasetEndReaded?.Invoke();
                OnSwitchFromDataset?.Invoke();
            }
            else
            {
                OnEndReadRow?.Invoke();
            }

            invocation.ReturnValue = !endOfSet;
        }

        public void ProxyReadAsync(IInvocation invocation)
        {
            Task<bool> originTask = null;
            try
            {
                invocation.Proceed();
                originTask = invocation.ReturnValue as Task<bool>;
            }
            catch
            {
                throw;
                // TODO
            }

            async Task<bool> AsyncWrapper()
            {
                // var reader = invocation.InvocationTarget as DbDataReader;

                OnStartReadRow?.Invoke();

                bool endOfSet;
                try
                {
                    endOfSet = !await originTask;
                }
                catch
                {
                    throw;
                    // TODO
                }

                if (endOfSet)
                {
                    OnDatasetEndReaded?.Invoke();
                    OnSwitchFromDataset?.Invoke();
                }
                else
                {
                    OnEndReadRow?.Invoke();
                }

                return !endOfSet;
            }
            invocation.ReturnValue = AsyncWrapper();
        }

        public void ProxyNextResult(IInvocation invocation)
        {
            OnSwitchFromDataset?.Invoke();

            invocation.Proceed();
            if ((bool)invocation.ReturnValue)
            {
                OnSwitchToDataset?.Invoke();
            }
        }

        public void ProxyNextResultAsync(IInvocation invocation)
        {
            OnSwitchFromDataset?.Invoke();

            Task<bool> originTask = null;
            try
            {
                invocation.Proceed();
                originTask = invocation.ReturnValue as Task<bool>;
            }
            catch
            {
                throw;
                // TODO
            }

            async Task<bool> AsyncWrap()
            {
                bool switched = false;
                try
                {
                    await originTask;
                }
                catch
                {
                    throw;
                    // TODO
                }

                if (switched)
                {
                    OnSwitchToDataset?.Invoke();
                }
                return switched;
            }
            invocation.ReturnValue = AsyncWrap();
        }
    }
}
