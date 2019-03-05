using System.Data.Common;
using TraceDbConnection.TraceReporting.Command;

namespace TraceDbConnection
{
    public interface ITraceReceiver
    {
        void Save(DbCommand command, ICommandTraceEntry traceEntry);
    }
}