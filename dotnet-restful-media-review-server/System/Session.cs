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

        private const int TIMEOUT_MINUTES = 30;

        private static readonly Dictionary<string, Session> _Sessions = new();

        private Session(string userName, string password)
        {
            UserName = userName;
            IsAdmin = (userName == "admin");

            Timestamp = DateTime.UtcNow;

            Token = string.Empty;
            Random rnd = new();
            for (int i = 0; i < 24; i++) { Token += _ALPHABET[rnd.Next(0, 62)]; }
        }

        public string Token { get; }

        public string UserName { get; }

        public DateTime Timestamp
        {
            get; private set;
        }

        public bool Valid
        {
            get { return _Sessions.ContainsKey(Token); }
        }

        public bool IsAdmin { get; }

        //Returns a session instance, or NULL if user couldn't be logged in.
        public static Session? Create(string userName, string password)
        {
            //todo: does the user exist, is the password correct??

            return new Session(userName, password);
        }

        //Returns the session represented by the token, or NULL if there is no session for the token.
        public static Session? Get(string token)
        {
            Session? rval = null;

            _Cleanup(); //cleanup all outdated sessions 

            lock (_Sessions)
            {
                if (_Sessions.ContainsKey(token))
                {
                    rval = _Sessions[token];
                    rval.Timestamp = DateTime.UtcNow; //refresh session timestamp
                }
            }


            return rval;
        }

        //Closes all outdated sessions.
        private static void _Cleanup()
        {
            List<string> toRemove = new();

            lock (_Sessions)
            {
                foreach (KeyValuePair<string, Session> pair in _Sessions)
                {
                    if ((DateTime.UtcNow - pair.Value.Timestamp).TotalMinutes > TIMEOUT_MINUTES) { toRemove.Add(pair.Key); }
                }
                foreach (string key in toRemove) { _Sessions.Remove(key); }
            }
        }

        public void Close()
        {
            lock (_Sessions)
            {
                if (_Sessions.ContainsKey(Token)) { _Sessions.Remove(Token); }
            }
        }
    }
}
