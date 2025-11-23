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

            if (context.Request.HasEntityBody)
            {
                using Stream input = context.Request.InputStream;
                using StreamReader re = new(input, context.Request.ContentEncoding);
                Body = re.ReadToEnd();
                Content = JsonNode.Parse(Body)?.AsObject() ?? new JsonObject();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Body);
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

        public Session? Session
        {
            get
            {
                string token = Context.Request.Headers["Authorization"] ?? string.Empty;
                if (token.ToLower().StartsWith("bearer "))
                {
                    token = token[7..].Trim();
                }
                else { return null; }

                return Session.Get(token);
            }
        }


        public bool Responded
        {
            get; set;
        } = false;

        public void Respond(HttpStatusCode statusCode, JsonObject? content)
        {
            Respond((int)statusCode, content);
        }

        public void Respond(int statusCode, JsonObject? content)
        {
            HttpListenerResponse response = Context.Response;
            response.StatusCode = statusCode;
            string rstr = content?.ToString() ?? string.Empty;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Responding: {statusCode}: {rstr}\n\n");

            byte[] buf = Encoding.UTF8.GetBytes(rstr);
            response.ContentLength64 = buf.Length;
            response.ContentType = "application/json; charset=UTF-8";

            using Stream output = response.OutputStream;
            output.Write(buf, 0, buf.Length);
            output.Close();

            Responded = true;
        }
    }
}
