using dotnet_restful_media_review_server.System;

namespace dotnet_restful_media_review_server.Database
{
    public static class UserRepository
    {
        public static bool CreateUser(User user)
        {
            const string sql = @"
                INSERT INTO users (username, password_hash, fullname, email)
                VALUES (@UserName, @PasswordHash, @FullName, @Email)
            ";

            return DB.Execute(sql, new
            {
                user.UserName,
                user.PasswordHash,
                user.FullName,
                user.Email
            }) == 1;
        }

        public static User? GetByUsername(string username)
        {
            const string sql = @"
                SELECT id,
                       username AS UserName,
                       password_hash AS PasswordHash,
                       fullname AS FullName,
                       email AS Email
                FROM users
                WHERE username = @username
            ";

            UserRecord? rec =
                DB.QuerySingleOrDefault<UserRecord>(sql, new { username });

            if (rec == null)
                return null;

            var user = new User
            {
                Id = rec.Id,
                UserName = rec.UserName,
                PasswordHash = rec.PasswordHash,
                FullName = rec.FullName,
                Email = rec.Email
            };

            user.MarkAsExisting();
            return user;
        }

        public static bool ValidateCredentials(
            string username,
            string password,
            out User? user)
        {
            user = GetByUsername(username);
            if (user == null) return false;
            return user.VerifyPassword(password);
        }
    }
}
