using Dapper;
using dotnet_restful_media_review_server.Database;
using dotnet_restful_media_review_server.Handlers;
using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using Npgsql;
using System.IO;
using System.Text.Json;

namespace dotnet_restful_media_review_server
{
    internal static class Program
    {
        class AppSettings
        {
            public ConnectionStrings ConnectionStrings { get; set; } = new();
        }

        class ConnectionStrings
        {
            public string DefaultConnection { get; set; } = string.Empty;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Program started");

            // Read connection string from appsettings.json manually
            // (TODO: there is for sure a better way to do this later)
            string jsonPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("ERROR: appsettings.json not found!");
                return;
            }

            string jsonText = File.ReadAllText(jsonPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(jsonText);
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionStrings.DefaultConnection))
            {
                Console.WriteLine("ERROR: Connection string not found in appsettings.json!");
                return;
            }

            string connString = settings.ConnectionStrings.DefaultConnection;

            // Test PostgreSQL connection (with docker)
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                Console.WriteLine("DB connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection test failed - is the Docker container running?: {ex.Message}");
            }

            // Configure Database class
            Database.Database.Configure(connString);

            // User DB operations - not important yet for intermediate handin
            //try
            //{
            //    var newUser = new User
            //    {
            //        UserName = "testuserr",
            //        FullName = "Test User",
            //        Email = "test@example.com"
            //    };
            //    newUser.SetPassword("secret123");

            //    bool created = UserRepository.CreateUser(newUser);
            //    Console.WriteLine($"User created: {created}");

            //    var loaded = UserRepository.GetByUsername("testuserr");
            //    if (loaded == null)
            //        Console.WriteLine("FAILED: Could not load user");
            //    else
            //        Console.WriteLine($"Loaded user: {loaded.UserName}, {loaded.Email}");

            //    bool correctLogin = UserRepository.ValidateCredentials("testuserr", "secret123", out var loggedIn);
            //    Console.WriteLine($"Correct password login: {correctLogin}");

            //    bool wrongLogin = UserRepository.ValidateCredentials("testuserr", "wrongpw", out _);
            //    Console.WriteLine($"Wrong password login: {wrongLogin}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"TEST ERROR: {ex.Message}");
            //}

            //Console.WriteLine("User DB test finished.");

            //  Start HTTP server
            var svr = new HttpRestServer();
            svr.RequestReceived += Handler.HandleEvent;
            svr.Run();
        }
    }
}
