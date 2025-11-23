using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using System.Net;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class UserHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (e.Path.StartsWith("/users"))
            {
                if ((e.Path == "/users") && (e.Method == HttpMethod.Post))
                {
                    try
                    {
                        User user = new()
                        {
                            UserName = e.Content?["username"]?.GetValue<string>() ?? string.Empty,
                            FullName = e.Content?["fullname"]?.GetValue<string>() ?? string.Empty,
                            EMail = e.Content?["email"]?.GetValue<string>() ?? string.Empty
                        };
                        user.SetPassword(e.Content?["password"]?.GetValue<string>() ?? string.Empty);

                        e.Respond(HttpStatusCode.OK, new JsonObject() { ["success"] = true, ["message"] = "User created." });

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject() { ["success"] = false, ["reason"] = ex.Message });
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(VersionHandler)} Exception creating user. {e.Method.ToString()} {e.Path}: {ex.Message}");
                    }
                }
                else
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject() { ["success"] = false, ["reason"] = "Invalid user endpoint." });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(VersionHandler)} Invalid user endpoint.");
                }

                e.Responded = true;
            }
        }
    }
}
