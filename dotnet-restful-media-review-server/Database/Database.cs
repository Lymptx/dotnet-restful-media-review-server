using System.Data;
using Npgsql;
using Dapper;
using System.Collections.Generic;

namespace dotnet_restful_media_review_server.Database
{
    public static class DB
    {
        private static string? _connectionString;

        public static void Configure(string connectionString)
        {
            _connectionString = connectionString;
        }

        private static IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException(
                    "Database not configured. Call DB.Configure(...) at startup."
                );

            return new NpgsqlConnection(_connectionString);
        }

        public static IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.Query<T>(sql, param);
        }

        public static T? QuerySingleOrDefault<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.QuerySingleOrDefault<T>(sql, param);
        }

        public static T QuerySingle<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.QuerySingle<T>(sql, param);
        }

        public static int Execute(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.Execute(sql, param);
        }
    }
}
