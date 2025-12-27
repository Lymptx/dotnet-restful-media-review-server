using dotnet_restful_media_review_server.Database;
using dotnet_restful_media_review_server.Handlers;
using dotnet_restful_media_review_server.Server;
using Npgsql;
using System.IO;
using System.Text.Json;

namespace dotnet_restful_media_review_server
{
    internal static class Program
    {
        private class AppSettings
        {
            public ConnectionStrings ConnectionStrings { get; set; } = new();
        }

        private class ConnectionStrings
        {
            public string DefaultConnection { get; set; } = string.Empty;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== Media Review Server Starting ===\n");

            string connString = LoadConnectionString();
            if (string.IsNullOrEmpty(connString))
            {
                Console.WriteLine("ERROR: Failed to load connection string. Exiting.");
                return;
            }

            if (!TestDatabaseConnection(connString))
            {
                Console.WriteLine("ERROR: Database connection failed. Exiting.");
                return;
            }

            DB.Configure(connString);

            StartHttpServer();
        }

        private static string LoadConnectionString()
        {
            string jsonPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"ERROR: appsettings.json not found at: {jsonPath}");
                return string.Empty;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(jsonText);

                if (settings?.ConnectionStrings?.DefaultConnection == null)
                {
                    Console.WriteLine("ERROR: Connection string not found in appsettings.json");
                    return string.Empty;
                }

                Console.WriteLine("✓ Configuration loaded");
                return settings.ConnectionStrings.DefaultConnection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to read appsettings.json: {ex.Message}");
                return string.Empty;
            }
        }

        private static bool TestDatabaseConnection(string connString)
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                Console.WriteLine("✓ Database connected");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Database connection failed: {ex.Message}");
                Console.WriteLine("Make sure Docker container is running!");
                return false;
            }
        }

        private static void StartHttpServer()
        {
            Console.WriteLine("\n=== Starting HTTP Server ===");
            var server = new HttpRestServer();
            server.RequestReceived += Handler.HandleEvent;
            Console.WriteLine("✓ Server listening on http://localhost:12000");
            Console.WriteLine("Press Ctrl+C to stop\n");
            server.Run();
        }
    }
}