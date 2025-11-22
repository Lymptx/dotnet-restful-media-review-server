using System.Reflection;

using dotnet_restful_media_review_server.Server;

namespace dotnet_restful_media_review_server.Handlers;

public abstract class Handler: IHandler
{
    private static List<IHandler> _Handlers = null;


    private static List<IHandler> _GetHandlers()
    {
        List<IHandler> handlersList = new();

        foreach(Type i in Assembly.GetExecutingAssembly().GetTypes()
            .Where(m => m.IsAssignableTo(typeof(IHandler)) && !m.IsAbstract))
        {
            IHandler? h = (IHandler?) Activator.CreateInstance(i);
            if(h is not null) { handlersList.Add(h); }
        }

        return handlersList;
    }

    //Event dispatcher
    public static void HandleEvent(object? sender, HttpRestEventArgs e)
    {
        foreach(IHandler i in (_Handlers ??= _GetHandlers()))
        {
            i.Handle(e);
            if(e.Responded) break;
        }
    }


    public abstract void Handle(HttpRestEventArgs e);
}