using dotnet_restful_media_review_server.Database;

namespace dotnet_restful_media_review_server.System
{
    public sealed class Session
    {
        private const string Alphabet = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int TimeoutMinutes = 30;

        private static readonly Dictionary<string, Session> Sessions = new();

        private Session(string userName)
        {
            UserName = userName;
            IsAdmin = userName == "admin";
            Timestamp = DateTime.UtcNow;

            var rnd = new Random();
            var chars = new char[24];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = Alphabet[rnd.Next(Alphabet.Length)];

            Token = new string(chars);
        }

        public string Token { get; }
        public string UserName { get; }
        public bool IsAdmin { get; }
        public DateTime Timestamp { get; private set; }

        public bool Valid => Sessions.ContainsKey(Token);

        public static Session? Create(string userName, string password)
        {
            if (!UserRepository.ValidateCredentials(userName, password, out _))
                return null;

            var session = new Session(userName);

            lock (Sessions)
                Sessions[session.Token] = session;

            return session;
        }

        public static Session? Get(string token)
        {
            Cleanup();

            lock (Sessions)
            {
                if (Sessions.TryGetValue(token, out var session))
                {
                    session.Timestamp = DateTime.UtcNow;
                    return session;
                }
            }

            return null;
        }

        private static void Cleanup()
        {
            var expired = new List<string>();

            lock (Sessions)
            {
                foreach (var pair in Sessions)
                {
                    if ((DateTime.UtcNow - pair.Value.Timestamp).TotalMinutes > TimeoutMinutes)
                        expired.Add(pair.Key);
                }

                foreach (var key in expired)
                    Sessions.Remove(key);
            }
        }

        public void Close()
        {
            lock (Sessions)
                Sessions.Remove(Token);
        }
    }
}
