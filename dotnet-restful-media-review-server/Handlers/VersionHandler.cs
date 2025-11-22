using System.Net;
using System.Text.Json.Nodes;
using dotnet_restful_media_review_server.Server;

namespace dotnet_restful_media_review_server.Handlers;

public sealed class VersionHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (e.Path.StartsWith("/version"))
        {
            if ((e.Path == "/version") && (e.Method == HttpMethod.Get))
            {
                //send valid response here
                JsonObject payload = new()
                {
                    ["success"] = true,
                    ["version"] = "1.0.0"
                };

                e.Respond(HttpStatusCode.OK, payload);
                return;
            }
            else
            {
                e.Respond(HttpStatusCode.BadRequest, new JsonObject() { ["success"] = false, ["reason"] = "Invalid version endpoint." });

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{nameof(VersionHandler)} Invalid session endpoint.");
            }
        }

        e.Responded = true;
    }
}