using System;
using System.Data.Common;
using System.Data.SqlClient;
using Castle.DynamicProxy;
using TraceDbConnection.TraceReporting.Command;

namespace TraceDbConnection.Interceptors
{
    internal class ConnectionInterceptor : InterceptorBase
    {
        public event Action<DbCommand, ICommandTraceEntry> OnDiagnostic;

        protected override bool ProxyCall(IInvocation invocation)
        {
            // more about CreateDbCommand:
            // https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection.createdbcommand?view=netframework-4.7.2
            if (invocation.Method.Name == nameof(SqlConnection.CreateCommand)
                || invocation.Method.Name == "CreateDbCommand")
            {
                ProxyCreateCommand(invocation);
                return true;
            }

            return base.ProxyCall(invocation);
        }

        private void ProxyCreateCommand(IInvocation invocation)
        {
            void OnDiagnosticCreated(ICommandTraceEntry traceEntry)
            {
                OnDiagnostic?.Invoke(invocation.ReturnValue as DbCommand, traceEntry);
            }
            var interceptor = new CommandInterceptor();
            interceptor.OnDiagnosticCreated += OnDiagnosticCreated;

            invocation.Proceed();

            var cmd = invocation.ReturnValue as DbCommand;

            var pGen = new ProxyGenerator();
            var cmdProxy = pGen.CreateClassProxyWithTarget(cmd, interceptor);
            invocation.ReturnValue = cmdProxy;
        }
    }
}