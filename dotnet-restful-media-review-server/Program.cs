using dotnet_restful_media_review_server.Handlers;
using dotnet_restful_media_review_server.Server;
using Npgsql;
using Dapper;

namespace dotnet_restful_media_review_server
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program started");

            // Test PostgreSQL connection
            try
            {
                // Hardcoded connection string
                string connString = "Host=localhost;Port=5433;Database=mrp;Username=mrp_user;Password=mrp_pwd";

                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                // Simple test query
                var userCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM users;");
                Console.WriteLine($"Users in database: {userCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection test failed: {ex.Message}");
            }

            // Start the server
            HttpRestServer svr = new();
            svr.RequestReceived += Handler.HandleEvent;
            svr.Run();
        }
    }
}
