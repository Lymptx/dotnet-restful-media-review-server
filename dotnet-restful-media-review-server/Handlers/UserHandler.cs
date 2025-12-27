using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class UserHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/users"))
                return;

            if (e.Path == "/users" && e.Method == HttpMethod.Get)
            {
                HandleGetAllUsers(e);
                return;
            }

            if (e.Path == "/users" && e.Method == HttpMethod.Post)
            {
                HandleCreateUser(e);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid user endpoint"
            });
            e.Responded = true;
        }

        private void HandleGetAllUsers(HttpRestEventArgs e)
        {
            try
            {
                var users = UserRepository.GetAllUsers();
                var arr = new JsonArray();

                foreach (var u in users)
                {
                    arr.Add(new JsonObject
                    {
                        ["id"] = u.Id,
                        ["username"] = u.UserName,
                        ["fullName"] = u.FullName,
                        ["email"] = u.Email
                    });
                }

                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve users"
                });
                Console.WriteLine($"Error in GetAllUsers: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleCreateUser(HttpRestEventArgs e)
        {
            try
            {
                string username = e.Content?["username"]?.GetValue<string>() ?? string.Empty;
                string name = e.Content?["name"]?.GetValue<string>() ?? string.Empty;
                string email = e.Content?["email"]?.GetValue<string>() ?? string.Empty;
                string password = e.Content?["password"]?.GetValue<string>() ?? string.Empty;

                // Validate input
                if (string.IsNullOrWhiteSpace(username))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Username is required"
                    });
                    e.Responded = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Password is required"
                    });
                    e.Responded = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Email is required"
                    });
                    e.Responded = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Full name is required"
                    });
                    e.Responded = true;
                    return;
                }

                // Check if username already exists
                var existing = UserRepository.GetByUsername(username);
                if (existing != null)
                {
                    e.Respond(HttpStatusCode.Conflict, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Username already exists"
                    });
                    e.Responded = true;
                    return;
                }

                // Create user
                User user = new()
                {
                    UserName = username,
                    FullName = name,
                    Email = email
                };
                user.SetPassword(password);

                bool created = UserRepository.CreateUser(user);

                if (created)
                {
                    e.Respond(HttpStatusCode.Created, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "User created successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Failed to create user"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while creating the user"
                });
                Console.WriteLine($"Error in CreateUser: {ex.Message}");
            }

            e.Responded = true;
        }
    }
}