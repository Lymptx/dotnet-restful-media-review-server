using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using System.Net;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class SessionHandler : Handler, IHandler
    {
        //Handles a request if possible
        public override void Handle(HttpRestEventArgs e)
        {
            if (e.Path.StartsWith("/sessions"))
            {
                if ((e.Path == "/sessions") && (e.Method == HttpMethod.Post))
                {
                    try
                    {
                        //create a session
                        Session? session = Session.Create(e.Content["username"]?.GetValue<string>() ?? string.Empty, e.Content["password"]?.GetValue<string>() ?? string.Empty);

                        if (session is not null)
                        {
                            //if seccion created successfuly, return JSON with a token
                            e.Respond(HttpStatusCode.OK, new JsonObject() { ["success"] = true, ["token"] = session.Token });
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
                        }
                        else
                        {
                            //return 401 Unauthorized 
                            e.Respond(HttpStatusCode.Unauthorized, new JsonObject() { ["success"] = false, ["reason"] = "Invalid username or password." });
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[{nameof(VersionHandler)} Invalid login attempt. {e.Method.ToString()} {e.Path}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject() { ["success"] = false, ["reason"] = ex.Message });
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(VersionHandler)} Exception creating session. {e.Method.ToString()} {e.Path}: {ex.Message}");
                    }
                }
                else
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject() { ["success"] = false, ["reason"] = "Invalid session endpoint." });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(VersionHandler)} Invalid session endpoint.");
                }

                e.Responded = true;
            }
        }
    }
}
