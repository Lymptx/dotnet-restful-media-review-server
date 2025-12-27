using dotnet_restful_media_review_server.System;
using Npgsql;

namespace dotnet_restful_media_review_server.Database
{
    public static class UserRepository
    {
        public static bool CreateUser(User user)
        {
            string sql = @"
                INSERT INTO users (username, full_name, email, password_hash, password_salt, created_at)
                VALUES (@username, @fullName, @email, @passwordHash, @passwordSalt, @createdAt)";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@username", user.UserName),
                new NpgsqlParameter("@fullName", user.FullName),
                new NpgsqlParameter("@email", user.Email),
                new NpgsqlParameter("@passwordHash", user.PasswordHash),
                new NpgsqlParameter("@passwordSalt", user.PasswordSalt),
                new NpgsqlParameter("@createdAt", user.CreatedAt)
            );

            return rows > 0;
        }

        public static User? GetByUsername(string username)
        {
            string sql = @"
                SELECT id, username, full_name, email, password_hash, password_salt, created_at
                FROM users
                WHERE username = @username";

            using var reader = DB.ExecuteReader(sql,
                new NpgsqlParameter("@username", username));

            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    UserName = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PasswordHash = reader.GetString(4),
                    PasswordSalt = reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                };
            }

            return null;
        }

        public static User? GetById(int id)
        {
            string sql = @"
                SELECT id, username, full_name, email, password_hash, password_salt, created_at
                FROM users
                WHERE id = @id";

            using var reader = DB.ExecuteReader(sql,
                new NpgsqlParameter("@id", id));

            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    UserName = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PasswordHash = reader.GetString(4),
                    PasswordSalt = reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                };
            }

            return null;
        }

        public static List<User> GetAllUsers()
        {
            string sql = @"
                SELECT id, username, full_name, email, password_hash, password_salt, created_at
                FROM users
                ORDER BY username";

            var users = new List<User>();

            using var reader = DB.ExecuteReader(sql);
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    UserName = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Email = reader.GetString(3),
                    PasswordHash = reader.GetString(4),
                    PasswordSalt = reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }

            return users;
        }

        public static bool ValidateCredentials(string username, string password, out User? user)
        {
            user = GetByUsername(username);
            if (user == null)
                return false;

            return user.VerifyPassword(password);
        }

        public static bool UpdateUser(User user)
        {
            string sql = @"
                UPDATE users
                SET full_name = @fullName, email = @email
                WHERE id = @id";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@fullName", user.FullName),
                new NpgsqlParameter("@email", user.Email),
                new NpgsqlParameter("@id", user.Id)
            );

            return rows > 0;
        }

        public static bool DeleteUser(int id)
        {
            string sql = "DELETE FROM users WHERE id = @id";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@id", id));

            return rows > 0;
        }
    }
}