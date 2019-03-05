using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using TraceDbConnection.TraceReporting.Command;
using TraceDbConnection.TraceReporting.Command.Reader;

namespace TraceDbConnection.Interceptors
{
    class CommandInterceptor: InterceptorBase
    {
        public event Action<ICommandTraceEntry> OnDiagnosticCreated;

        protected override bool ProxyCall(IInvocation invocation)
        {
            if (invocation.Method.Name == nameof(DbCommand.ExecuteReader)
                || invocation.Method.Name == "ExecuteDbDataReader")
            {
                ProxyExecuteReader(invocation);
                return true;
            }

            return base.ProxyCall(invocation);
        }

        protected override bool ProxyCallAsync(IInvocation invocation)
        {
            if (invocation.Method.Name == nameof(DbCommand.ExecuteReaderAsync)
                || invocation.Method.Name == "ExecuteDbDataReaderAsync")
            {
                ProxyExecuteReaderAsync(invocation);
                return true;
            }

            return base.ProxyCallAsync(invocation);
        }

        private void ProxyExecuteReader(IInvocation invocation)
        {
            ReaderTraceEntry readerTraceEntry = null;
            readerTraceEntry = new ReaderTraceEntry();
            OnDiagnosticCreated?.Invoke(readerTraceEntry);

            DbDataReader reader = null;
            {
                try
                {
                    var openReaderTime = new Stopwatch();

                    openReaderTime.Start();
                    invocation.Proceed();
                    openReaderTime.Stop();

                    readerTraceEntry.OpenTime = openReaderTime.Elapsed;

                    reader = invocation.ReturnValue as DbDataReader;
                }
                catch
                {
                    // TODO
                    throw;
                }
            }

            var readerInterceptor = new ReaderInterceptor();
            HookDatasetActions(readerInterceptor, readerTraceEntry);

            var pGen = new ProxyGenerator();
            var proxyObj = pGen.CreateClassProxyWithTarget(reader, readerInterceptor);
            invocation.ReturnValue = proxyObj;
        }

        private void ProxyExecuteReaderAsync(IInvocation invocation)
        {
            var readerTraceEntry = new ReaderTraceEntry();
            OnDiagnosticCreated?.Invoke(readerTraceEntry);

            var openReaderTime = new Stopwatch();
            Task<DbDataReader> originTask = null;
            try
            {
                openReaderTime.Start();
                invocation.Proceed();

                originTask = invocation.ReturnValue as Task<DbDataReader>;
            }
            catch
            {
                throw;
                // TODO
            }

            async Task<DbDataReader> AsyncWrapper()
            {
                DbDataReader readerOrigin = null;
                try
                {
                    readerOrigin = await originTask;
                    openReaderTime.Stop();
                    readerTraceEntry.OpenTime = openReaderTime.Elapsed;
                }
                catch
                {
                    throw;
                    // TODO
                }

                var readerInterceptor = new ReaderInterceptor();
                HookDatasetActions(readerInterceptor, readerTraceEntry);

                var pGen = new ProxyGenerator();
                var proxyObj = pGen.CreateClassProxyWithTarget(readerOrigin, readerInterceptor);
                return proxyObj;
            }
            invocation.ReturnValue = AsyncWrapper();
        }

        private void HookDatasetActions(ReaderInterceptor readerInterceptor, ReaderTraceEntry readerTraceEntry)
        {
            DatasetTraceEntry datasetTraceEntry = null;
            var rowReadTime = new Stopwatch();
            void OnStartReadRow()
            {
                if (datasetTraceEntry == null)
                {
                    OnSwitchToDataset();
                }
                rowReadTime.Start();
            }

            void OnEndReadRow()
            {
                rowReadTime.Stop();
                datasetTraceEntry.AddRowReadTime(rowReadTime.Elapsed);
                rowReadTime.Reset();
            }

            void OnDatasetEndReaded()
            {
                datasetTraceEntry.ReadCompleted();
                OnSwitchFromDataset();
            }

            void OnSwitchToDataset()
            {
                rowReadTime.Reset();
                datasetTraceEntry = new DatasetTraceEntry();
                readerTraceEntry.AddDatasetTrace(datasetTraceEntry);
            }

            void OnSwitchFromDataset()
            {
                datasetTraceEntry = null;
            }

            readerInterceptor.OnStartReadRow += OnStartReadRow;
            readerInterceptor.OnEndReadRow += OnEndReadRow;
            readerInterceptor.OnDatasetEndReaded += OnDatasetEndReaded;

            readerInterceptor.OnSwitchFromDataset += OnSwitchFromDataset;
            readerInterceptor.OnSwitchToDataset += OnSwitchToDataset;
        }
    }
}
