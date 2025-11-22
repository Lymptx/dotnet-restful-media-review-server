using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_restful_media_review_server.System
{
    public sealed class Session
    {
        private const string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static readonly Dictionary<string, Session> _Sessions = new();

        private Session(string userName, string password)
        {
            UserName = userName;
            IsAdmin = (userName == "admin");

            Token = string.Empty;
            Random rnd = new();
            for (int i = 0; i < 24; i++) { Token += _ALPHABET[rnd.Next(0, 62)]; }
        }


        public string Token { get; }

        public string UserName { get; }

        public bool Valid
        {
            get { return _Sessions.ContainsKey(Token); }
        }

        public bool IsAdmin { get; }

        public void Close()
        {
            lock (_Sessions)
            {
                if (_Sessions.ContainsKey(Token)) { _Sessions.Remove(Token); }
            }
        }
    }
}
