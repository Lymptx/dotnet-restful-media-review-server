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
                try
                {
                    string username = e.Content["username"]?.GetValue<string>() ?? string.Empty;
                    string password = e.Content["password"]?.GetValue<string>() ?? string.Empty;

                    Session? session = Session.Create(username, password);

                    if (session == null)
                    {
                        e.Respond(
                            HttpStatusCode.Unauthorized,
                            new JsonObject
                            {
                                ["success"] = false,
                                ["reason"] = "Invalid username or password"
                            }
                        );
                    }
                    else
                    {
                        e.Respond(
                            HttpStatusCode.OK,
                            new JsonObject
                            {
                                ["success"] = true,
                                ["token"] = session.Token
                            }
                        );
                    }
                }
                catch (Exception ex)
                {
                    e.Respond(
                        HttpStatusCode.InternalServerError,
                        new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = ex.Message
                        }
                    );
                }

                e.Responded = true;
                return;
            }

            e.Respond(
                HttpStatusCode.BadRequest,
                new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Invalid session endpoint"
                }
            );

            e.Responded = true;
        }
    }
}
