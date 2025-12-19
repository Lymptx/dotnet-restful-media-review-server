using dotnet_restful_media_review_server.Database;

namespace dotnet_restful_media_review_server.System
{
    public sealed class Session
    {
        private const string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int TIMEOUT_MINUTES = 30;

        private static readonly Dictionary<string, Session> _Sessions = new();

        private Session(string userName)
        {
            UserName = userName;
            IsAdmin = (userName == "admin");

            Timestamp = DateTime.UtcNow;

            Random rnd = new();
            Token = string.Empty;
            for (int i = 0; i < 24; i++)
            {
                Token += _ALPHABET[rnd.Next(0, _ALPHABET.Length)];
            }
        }

        public string Token { get; }
        public string UserName { get; }
        public bool IsAdmin { get; }

        public DateTime Timestamp { get; private set; }

        public bool Valid => _Sessions.ContainsKey(Token);

        public static Session? Create(string userName, string password)
        {
            // 1. Validate credentials against DB
            if (!UserRepository.ValidateCredentials(userName, password, out var user))
                return null;

            // 2. Create session
            var session = new Session(user.UserName);

            // 3. Store session
            lock (_Sessions)
            {
                _Sessions[session.Token] = session;
            }

            return session;
        }
        public static Session? Get(string token)
        {
            _Cleanup();

            lock (_Sessions)
            {
                if (_Sessions.TryGetValue(token, out var session))
                {
                    session.Timestamp = DateTime.UtcNow; // refresh
                    return session;
                }
            }

            return null;
        }
        private static void _Cleanup()
        {
            var toRemove = new List<string>();

            lock (_Sessions)
            {
                foreach (var pair in _Sessions)
                {
                    if ((DateTime.UtcNow - pair.Value.Timestamp).TotalMinutes > TIMEOUT_MINUTES)
                        toRemove.Add(pair.Key);
                }

                foreach (var key in toRemove)
                    _Sessions.Remove(key);
            }
        }
        public void Close()
        {
            lock (_Sessions)
            {
                _Sessions.Remove(Token);
            }
        }
    }
}
