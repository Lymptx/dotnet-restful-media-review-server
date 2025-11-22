using dotnet_restful_media_review_server.Handlers;
using dotnet_restful_media_review_server.Server;

static void Main(string[] args)
{
    Console.WriteLine("Program started");
    HttpRestServer svr = new();
    svr.RequestReceived += Handler.HandleEvent;
    svr.Run();
}
