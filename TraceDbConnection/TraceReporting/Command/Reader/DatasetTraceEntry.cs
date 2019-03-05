using System;

namespace TraceDbConnection.TraceReporting.Command.Reader
{
    public class DatasetTraceEntry
    {
        public bool ReadedAll { get; private set; }
        public ulong RowsReaded { get; private set; }

        public TimeSpan? AvgRowReadTime => RowsReaded == 0
            ? (TimeSpan?)null
            : TimeSpan.FromTicks((long)Math.Floor(TotalReadTime.Ticks / (decimal)RowsReaded));

        public TimeSpan? MaxRowReadTime { get; private set; }

        public TimeSpan TotalReadTime { get; private set; }  = TimeSpan.Zero;

        private bool _readCompleted;

        public void AddRowReadTime(TimeSpan time)
        {
            if (_readCompleted)
                throw new InvalidOperationException("Read is completed.");

            TotalReadTime += time;

            MaxRowReadTime = RowsReaded == 0 || !MaxRowReadTime.HasValue || MaxRowReadTime < time
                ? time
                : MaxRowReadTime;

            RowsReaded++;
        }

        public void ReadCompleted(bool readedToEnd = true)
        {
            if (_readCompleted) return;

            _readCompleted = true;
            ReadedAll = readedToEnd;
        }
    }
}