using dotnet_restful_media_review_server.System;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Server
{
    public class HttpRestEventArgs : EventArgs
    {
        public HttpRestEventArgs(HttpListenerContext context)
        {
            Context = context;
            Method = HttpMethod.Parse(context.Request.HttpMethod);
            Path = context.Request.Url?.AbsolutePath ?? string.Empty;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Received: {Method} {Path}");
            Console.ResetColor();

            // Read request body if present
            if (context.Request.HasEntityBody)
            {
                using Stream input = context.Request.InputStream;
                using StreamReader reader = new(input, context.Request.ContentEncoding);
                Body = reader.ReadToEnd();
                Content = JsonNode.Parse(Body)?.AsObject() ?? new JsonObject();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Body);
                Console.ResetColor();
            }
            else
            {
                Body = string.Empty;
                Content = new JsonObject();
            }
        }

        public HttpListenerContext Context { get; }
        public HttpMethod Method { get; }
        public string Path { get; }
        public string Body { get; }
        public JsonObject Content { get; }
        public bool Responded { get; set; }

        // Extracts session from Authorization header if present
        public Session? Session
        {
            get
            {
                string token = Context.Request.Headers["Authorization"] ?? string.Empty;

                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token[7..].Trim(); // Remove "Bearer " prefix
                    return System.Session.Get(token);
                }

                return null;
            }
        }

        public void Respond(HttpStatusCode statusCode, JsonNode? content)
        {
            HttpListenerResponse response = Context.Response;
            response.StatusCode = (int)statusCode;

            string body = content?.ToJsonString() ?? string.Empty;

            // Log response for debugging
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Responding: {statusCode} ({(int)statusCode})");
            Console.WriteLine(body);
            Console.WriteLine();
            Console.ResetColor();

            // Send JSON response
            byte[] buffer = Encoding.UTF8.GetBytes(body);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json; charset=UTF-8";

            using Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            Responded = true;
        }

        // Overload for int status code
        public void Respond(int statusCode, JsonNode? content)
        {
            Respond((HttpStatusCode)statusCode, content);
        }
    }
}