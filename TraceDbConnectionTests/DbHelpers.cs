using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace TraceDbConnectionTests
{
    static class DbHelpers
    {
        public static void ReadRows(this DbCommand cmd, uint? count = null)
        {
            using (var reader = cmd.ExecuteReader())
            {
                uint readedRows = 0;
                while (true)
                {
                    if (readedRows >= (count ?? uint.MaxValue)) break;
                    if (!reader.Read()) break;
                }
            }
        }

        public static async Task ReadRowsAsync(this DbCommand cmd, uint? count = null)
        {
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                uint readedRows = 0;
                while (true)
                {
                    if (readedRows >= (count ?? uint.MaxValue)) break;
                    if (!await reader.ReadAsync(CancellationToken.None)) break;
                }
            }
        }
    }
}
