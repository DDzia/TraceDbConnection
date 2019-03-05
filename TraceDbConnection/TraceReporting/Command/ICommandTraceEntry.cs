namespace TraceDbConnection.TraceReporting.Command
{
    public interface ICommandTraceEntry
    {
        CommandTraceObject TraceObject { get; }
    }
}