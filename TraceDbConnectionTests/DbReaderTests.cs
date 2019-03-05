using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using TraceDbConnection;
using TraceDbConnection.TraceReporting.Command;
using TraceDbConnection.TraceReporting.Command.Reader;
using Xunit;

namespace TraceDbConnectionTests
{
    public class DbReaderTests: IDisposable
    {
        private readonly Mock<ITraceReceiver> _storeMock;
        private readonly ISetup<ITraceReceiver> _saveMock;
        private readonly DbConnection _sut;

        public DbReaderTests()
        {
            DbConfig.ReInitDatabase();

            _storeMock = new Mock<ITraceReceiver>();
            _saveMock = _storeMock.Setup(x => x.Save(It.IsAny<DbCommand>(), It.IsAny<ICommandTraceEntry>()));

            _sut = TraceConnector.Connect(DbConfig.CreateConnectionAndOpen(), _storeMock.Object);
        }

        public void Dispose()
        {
            _sut.Close();
        }

        [Fact]
        public void ShouldSaveReaderDiagnostic_When_WhenOneSetIsReadedFully()
        {
            // Arrange
            ICommandTraceEntry traceEntryRead = null;
            DbCommand commandRead = null;
            _saveMock.Callback<DbCommand, ICommandTraceEntry>((command, diagnostics) =>
            {
                commandRead = command;
                traceEntryRead = diagnostics;
            });

            // Act
            var cmd = _sut.CreateCommand();
            using (cmd)
            {
                cmd.CommandText = $"SELECT * FROM {DbConfig.DbObjectNames.UsersTable}";
                cmd.ReadRows();
            }

            // Assert
            _storeMock.Verify(x => x.Save(It.IsNotNull<DbCommand>(), It.IsNotNull<ICommandTraceEntry>()), Times.Once);
            Assert.Equal(cmd, commandRead);
            Assert.NotNull(traceEntryRead);
            Assert.Equal(CommandTraceObject.Reader, traceEntryRead.TraceObject);

            var readerDInfo = traceEntryRead as ReaderTraceEntry;
            Assert.NotNull(readerDInfo.OpenTime);
            Assert.NotEmpty(readerDInfo.TracesOfDatasets);
            Assert.Single(readerDInfo.TracesOfDatasets);

            var firstSetDInfo = readerDInfo.TracesOfDatasets.ElementAt(0);
            Assert.True(firstSetDInfo.ReadedAll);
            Assert.Equal((ulong)3, firstSetDInfo.RowsReaded);
            Assert.True(firstSetDInfo.TotalReadTime > TimeSpan.Zero);
            Assert.True(firstSetDInfo.AvgRowReadTime > TimeSpan.Zero);
            Assert.True(firstSetDInfo.MaxRowReadTime > TimeSpan.Zero);
        }

        [Fact]
        public async Task ShouldSaveReaderDiagnostic_When_WhenOneSetIsReadedFullyAsynchronous()
        {
            // Arrange
            ICommandTraceEntry traceEntryRead = null;
            DbCommand commandRead = null;
            _saveMock.Callback<DbCommand, ICommandTraceEntry>((command, diagnostics) =>
            {
                commandRead = command;
                traceEntryRead = diagnostics;
            });

            // Act
            var cmd = _sut.CreateCommand();
            using (cmd)
            {
                cmd.CommandText = $"SELECT * FROM {DbConfig.DbObjectNames.UsersTable}";
                await cmd.ReadRowsAsync();
            }

            // Assert
            _storeMock.Verify(x => x.Save(It.IsNotNull<DbCommand>(), It.IsNotNull<ICommandTraceEntry>()), Times.Once);
            Assert.Equal(cmd, commandRead);
            Assert.NotNull(traceEntryRead);
            Assert.Equal(CommandTraceObject.Reader, traceEntryRead.TraceObject);

            var readerDInfo = traceEntryRead as ReaderTraceEntry;
            Assert.NotNull(readerDInfo.OpenTime);
            Assert.NotEmpty(readerDInfo.TracesOfDatasets);
            Assert.Single(readerDInfo.TracesOfDatasets);

            var firstSetDInfo = readerDInfo.TracesOfDatasets.ElementAt(0);
            Assert.True(firstSetDInfo.ReadedAll);
            Assert.Equal((ulong)3, firstSetDInfo.RowsReaded);
            Assert.True(firstSetDInfo.TotalReadTime > TimeSpan.Zero);
            Assert.True(firstSetDInfo.AvgRowReadTime > TimeSpan.Zero);
            Assert.True(firstSetDInfo.MaxRowReadTime > TimeSpan.Zero);
        }
    }
}
