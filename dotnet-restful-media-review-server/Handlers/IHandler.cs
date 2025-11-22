using dotnet_restful_media_review_server.Server;

namespace dotnet_restful_media_review_server.Handlers;

public interface IHandler
{
    public void Handle(HttpRestEventArgs e);
}