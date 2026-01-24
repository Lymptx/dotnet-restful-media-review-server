using dotnet_restful_media_review_server.System;
using Npgsql;

namespace dotnet_restful_media_review_server.Database
{
    public static class FavoriteRepository
    {
        public static bool AddFavorite(int userId, int mediaId)
        {
            string sql = @"
                INSERT INTO favorites (user_id, media_id, created_at)
                VALUES (@userId, @mediaId, @createdAt)
                ON CONFLICT (user_id, media_id) DO NOTHING";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@mediaId", mediaId),
                new NpgsqlParameter("@createdAt", DateTime.UtcNow)
            );

            return rows > 0;
        }

        public static bool RemoveFavorite(int userId, int mediaId)
        {
            string sql = @"
                DELETE FROM favorites
                WHERE user_id = @userId AND media_id = @mediaId";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@mediaId", mediaId)
            );

            return rows > 0;
        }

        public static bool IsFavorite(int userId, int mediaId)
        {
            string sql = @"
                SELECT COUNT(*) FROM favorites
                WHERE user_id = @userId AND media_id = @mediaId";

            object? result = DB.ExecuteScalar(sql,
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@mediaId", mediaId)
            );

            return result != null && Convert.ToInt32(result) > 0;
        }

        public static List<MediaEntry> GetUserFavorites(int userId)
        {
            string sql = @"
                SELECT m.id, m.title, m.media_type, m.genre, m.release_year, 
                       m.age_restriction, m.description, m.creator_user_id, 
                       m.created_at, m.updated_at
                FROM media_entries m
                INNER JOIN favorites f ON m.id = f.media_id
                WHERE f.user_id = @userId
                ORDER BY f.created_at DESC";

            var favorites = new List<MediaEntry>();

            using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@userId", userId));
            while (reader.Read())
            {
                favorites.Add(new MediaEntry
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    MediaType = reader.GetString(2),
                    Genre = reader.GetString(3),
                    ReleaseYear = reader.GetInt32(4),
                    AgeRestriction = reader.GetInt32(5),
                    Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    CreatorUserId = reader.GetInt32(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                });
            }

            return favorites;
        }

        public static int GetFavoriteCount(int userId)
        {
            string sql = "SELECT COUNT(*) FROM favorites WHERE user_id = @userId";

            object? result = DB.ExecuteScalar(sql, new NpgsqlParameter("@userId", userId));

            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}