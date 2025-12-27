using System.Net;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Server
{
    public sealed class HttpRestServer : IDisposable
    {
        private readonly HttpListener _listener;
        private bool _disposed;

        public HttpRestServer(int port = 12000)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{port}/");
        }

        public event EventHandler<HttpRestEventArgs>? RequestReceived;

        public bool Running { get; private set; }

        public void Run()
        {
            if (Running)
                return;

            _listener.Start();
            Running = true;

            // Main request loop
            while (Running)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();

                    // Handle each request in a separate thread
                    _ = Task.Run(() => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Expected when stopping listener
                    if (Running)
                        throw;
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                HttpRestEventArgs args = new(context);
                RequestReceived?.Invoke(this, args);

                // Send 404 if no handler responded
                if (!args.Responded)
                {
                    args.Respond(
                        HttpStatusCode.NotFound,
                        new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = "Endpoint not found"
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error handling request: {ex.Message}");
                Console.ResetColor();
            }
        }

        public void Stop()
        {
            if (!Running)
                return;

            Running = false;
            _listener.Stop();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _listener.Close();
            _disposed = true;
        }
    }
}