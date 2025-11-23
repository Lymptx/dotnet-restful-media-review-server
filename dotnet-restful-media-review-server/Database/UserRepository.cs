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
            try
            {
                int rows = Database.Execute(sql, new
                {
                    UserName = user.UserName,
                    PasswordHash = user.PasswordHash,
                    FullName = user.FullName,
                    Email = user.Email
                });
                return rows == 1;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") // unique_violation
            {
                return false;
            }
        }

        public static User? GetByUsername(string username)
        {
            const string sql = @"
                SELECT id, username as UserName, password_hash as PasswordHash, fullname as FullName, email as Email, created_at as CreatedAt
                FROM users WHERE username = @username
            ";
            return Database.QuerySingleOrDefault<User>(sql, new { username });
        }

        public static bool ValidateCredentials(string username, string password, out User? user)
        {
            user = GetByUsername(username);
            if (user == null) return false;
            return user.VerifyPassword(password);
        }
    }
}
