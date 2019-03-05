using System.Data.SqlClient;
using System.IO;

namespace TraceDbConnectionTests
{
    internal static class DbConfig
    {
        private const string DatabaseName = "SqlServerQueryProfilerTests";

        private static readonly string ConnectionStringWithoutDb = @"Server=(localdb)\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=True;Pooling=false;";

        private static readonly string ConnectionString = $"{ConnectionStringWithoutDb}Database={DatabaseName};";

        public static SqlConnection CreateConnectionAndOpen()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private static void ExecuteScript(this SqlConnection conn, string cmdText)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();
        }

        private static void ExecuteScriptFromFile(this SqlConnection conn, string filePath)
        {
            var root = Path.GetDirectoryName(typeof(DbConfig).Assembly.Location);
            var absolutePath = Path.Combine(root, filePath);
            var sql = File.ReadAllText(absolutePath);
            conn.ExecuteScript(sql);
        }

        public static void ReInitDatabase()
        {
            DropDatabase();
            CreateDatabase();
            PopulateDatabaseData();
        }

        private static void DropDatabase()
        {
            using (var conn = new SqlConnection(ConnectionStringWithoutDb))
            {
                conn.Open();
                var recreateDbPhysicaly = "USE master\n" +
                                          $"IF EXISTS(select * from sys.databases where name='{DatabaseName}')\n" +
                                          $"DROP DATABASE {DatabaseName}";
                conn.ExecuteScript(recreateDbPhysicaly);
            }
        }

        private static void CreateDatabase()
        {
            using (var conn = new SqlConnection(ConnectionStringWithoutDb))
            {
                conn.Open();
                var recreateDbPhysicaly = "USE master\n" +
                                          $"CREATE DATABASE {DatabaseName}";
                conn.ExecuteScript(recreateDbPhysicaly);
            }
        }

        private static void PopulateDatabaseData()
        {
            using (var conn = CreateConnectionAndOpen())
            {
                conn.ExecuteScriptFromFile("scripts/create-users-table.sql");
                conn.ExecuteScriptFromFile("scripts/populate-users.sql");
            }
        }

        public static class DbObjectNames
        {
            public const string UsersTable = "tUsers";
        }
    }
}
