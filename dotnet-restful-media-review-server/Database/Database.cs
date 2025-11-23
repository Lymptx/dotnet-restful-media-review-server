using System.Data;
using Npgsql;
using Dapper;

namespace dotnet_restful_media_review_server.Database
{
    public static class Database
    {
        private static string? _connectionString;

        public static void Configure(string connectionString)
        {
            _connectionString = connectionString;
        }

        private static IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database not configured. Call Database.Configure(...) at startup.");
            return new NpgsqlConnection(_connectionString);
        }

        public static T? QuerySingleOrDefault<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.QuerySingleOrDefault<T>(sql, param);
        }

        public static int Execute(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.Execute(sql, param);
        }

        public static T QuerySingle<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.QuerySingle<T>(sql, param);
        }
    }
}
