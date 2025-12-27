using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using System.Net;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class SessionHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/sessions"))
                return;

            if (e.Path == "/sessions" && e.Method == HttpMethod.Post)
            {
                HandleLogin(e);
                return;
            }

            if (e.Path == "/sessions" && e.Method == HttpMethod.Delete)
            {
                HandleLogout(e);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid session endpoint"
            });
            e.Responded = true;
        }

        private void HandleLogin(HttpRestEventArgs e)
        {
            try
            {
                string username = e.Content["username"]?.GetValue<string>() ?? string.Empty;
                string password = e.Content["password"]?.GetValue<string>() ?? string.Empty;

                // Validate input
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Username and password are required"
                    });
                    e.Responded = true;
                    return;
                }

                // Attempt to create session
                Session? session = Session.Create(username, password);

                if (session == null)
                {
                    e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Invalid username or password"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["token"] = session.Token
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred during login"
                });
                Console.WriteLine($"Error in Login: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleLogout(HttpRestEventArgs e)
        {
            try
            {
                Session? session = e.Session;

                if (session == null)
                {
                    e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Not logged in or invalid token"
                    });
                    e.Responded = true;
                    return;
                }

                session.Close();

                e.Respond(HttpStatusCode.OK, new JsonObject
                {
                    ["success"] = true,
                    ["message"] = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred during logout"
                });
                Console.WriteLine($"Error in Logout: {ex.Message}");
            }

            e.Responded = true;
        }
    }
}