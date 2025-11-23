using Dapper;
using dotnet_restful_media_review_server.Database;
using dotnet_restful_media_review_server.Handlers;
using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using Npgsql;

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

            // configure db
            Database.Database.Configure("Host=localhost;Port=5433;Database=mrp;Username=mrp_user;Password=mrp_pwd;Pooling=true;");

            //temporary test code for testing user functionality
            var user = new User { UserName = "batman", FullName = "Bruce Wayne", Email = "batman@gotham.com" };
            user.SetPassword("batcave");
            bool created = UserRepository.CreateUser(user);
            Console.WriteLine($"User created: {created}");

            var fetched = UserRepository.GetByUsername("batman");
            Console.WriteLine($"Fetched user: {fetched?.UserName}, email: {fetched?.Email}");

            bool validLogin = UserRepository.ValidateCredentials("batman", "batcave", out var loggedUser);
            Console.WriteLine($"Login valid: {validLogin}, Name: {loggedUser?.FullName}");

            // Start the server
            HttpRestServer svr = new();
            svr.RequestReceived += Handler.HandleEvent;
            svr.Run();



        }
    }
}
