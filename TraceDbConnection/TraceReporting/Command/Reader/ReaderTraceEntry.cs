using System;
using System.Collections.Generic;

namespace TraceDbConnection.TraceReporting.Command.Reader
{
    public class ReaderTraceEntry: ICommandTraceEntry
    {
        public CommandTraceObject TraceObject => CommandTraceObject.Reader;

        public TimeSpan? OpenTime = null;

        public TimeSpan TotalReadTime
        {
            get
            {
                var timeSum = OpenTime ?? TimeSpan.Zero;
                foreach (var t in TracesOfDatasets)
                {
                    timeSum += t.TotalReadTime;
                }

                return timeSum;
            }
        }

        
        public IReadOnlyCollection<DatasetTraceEntry> TracesOfDatasets => _tracesOfDatasets;

        private readonly List<DatasetTraceEntry> _tracesOfDatasets = new List<DatasetTraceEntry>();

        public void AddDatasetTrace(DatasetTraceEntry datasetTraceEntry)
        {
            _tracesOfDatasets.Add(datasetTraceEntry);
        }
    }
}