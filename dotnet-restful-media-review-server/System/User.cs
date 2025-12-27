using System.Security.Cryptography;
using System.Text;

namespace dotnet_restful_media_review_server.System
{
    public sealed class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty");

            PasswordSalt = GenerateSalt();
            PasswordHash = HashPassword(password, PasswordSalt);
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            string hash = HashPassword(password, PasswordSalt);
            return hash == PasswordHash;
        }

        private static string GenerateSalt()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combined = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

            byte[] hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }
    }
}