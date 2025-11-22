using dotnet_restful_media_review_server.Handlers;
using dotnet_restful_media_review_server.Server;

namespace dotnet_restful_media_review_server
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program started");
            HttpRestServer svr = new();
            svr.RequestReceived += Handler.HandleEvent;
            svr.Run();
        }
    }
}
