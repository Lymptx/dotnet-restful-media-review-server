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

            // GET /users
            if (e.Path == "/users" && e.Method == HttpMethod.Get)
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
                e.Responded = true;
                return;
            }

            // POST /users
            if (e.Path == "/users" && e.Method == HttpMethod.Post)
            {
                try
                {
                    User user = new()
                    {
                        UserName = e.Content?["username"]?.GetValue<string>() ?? string.Empty,
                        FullName = e.Content?["name"]?.GetValue<string>() ?? string.Empty,
                        Email = e.Content?["email"]?.GetValue<string>() ?? string.Empty
                    };

                    user.SetPassword(
                        e.Content?["password"]?.GetValue<string>() ?? string.Empty
                    );

                    UserRepository.CreateUser(user);

                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "User created."
                    });
                }
                catch (Exception ex)
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = ex.Message
                    });
                }

                e.Responded = true;
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid user endpoint."
            });

            e.Responded = true;
        }
    }
}
