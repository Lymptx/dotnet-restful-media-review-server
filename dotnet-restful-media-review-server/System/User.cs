using System.Security.Cryptography;
using System.Text;

namespace dotnet_restful_media_review_server.System
{
    public sealed class User : Atom, IAtom
    {
        public int Id { get; set; } = 0;
        private string? _UserName = null;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        private bool _New;

        public User(Session? session = null)
        {
            _EditingSession = session;
            _New = true;
        }

        // Parameterless constructor required for Dapper for now, later a construcur where the parameters exactly match the column names and types
        public User() { }



        public static User Get(string userName, Session? session = null)
        {
            // TODO: load user and return if admin or owner.
            throw new NotImplementedException();
        }


        public string UserName
        {
            get { return _UserName ?? string.Empty; }
            set
            {
                if (!_New) { throw new InvalidOperationException("User name cannot be changed."); }
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("User name must not be empty."); }

                _UserName = value;
            }
        }

        public bool VerifyPassword(string password)
        {
            string h = HashPassword(UserName, password);
            return string.Equals(h, PasswordHash, StringComparison.OrdinalIgnoreCase);
        }

        internal static string HashPassword(string username, string password)
        {
            // username used as salt
            using var sha = SHA256.Create();
            byte[] input = Encoding.UTF8.GetBytes(username + password);
            byte[] hash = sha.ComputeHash(input);
            StringBuilder sb = new();
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public void SetPassword(string password)
        {
            PasswordHash = HashPassword(UserName, password);
        }

        public override void Save()
        {
            if (!_New) { _EnsureAdminOrOwner(UserName); }

            // TODO: save user to database
            PasswordHash = null;
            _EndEdit();
        }

        public override void Delete()
        {
            _EnsureAdminOrOwner(UserName);

            // TODO: delete user from database

            _EndEdit();
        }

        public override void Refresh()
        {
            // TODO: refresh user from database
            _EndEdit();
        }
    }
}
