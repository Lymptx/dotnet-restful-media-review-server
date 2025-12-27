using System.Data;
using Npgsql;

namespace dotnet_restful_media_review_server.Database
{
    public static class DB
    {
        private static string? _connectionString;

        public static void Configure(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static NpgsqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException(
                    "Database not configured. Call DB.Configure(...) at startup."
                );
            return new NpgsqlConnection(_connectionString);
        }

        public static int ExecuteNonQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static NpgsqlDataReader ExecuteReader(string sql, params NpgsqlParameter[] parameters)
        {
            var conn = GetConnection();
            conn.Open();
            var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}