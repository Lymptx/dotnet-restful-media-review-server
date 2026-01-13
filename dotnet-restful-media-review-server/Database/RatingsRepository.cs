using dotnet_restful_media_review_server.System;
using Npgsql;

namespace dotnet_restful_media_review_server.Database
{
    public static class RatingRepository
    {
        public static bool CreateRating(Rating rating)
        {
            string sql = @"
                INSERT INTO ratings (media_id, user_id, stars, comment, is_confirmed, created_at)
                VALUES (@mediaId, @userId, @stars, @comment, @isConfirmed, @createdAt)
                RETURNING id";

            object? result = DB.ExecuteScalar(sql,
                new NpgsqlParameter("@mediaId", rating.MediaId),
                new NpgsqlParameter("@userId", rating.UserId),
                new NpgsqlParameter("@stars", rating.Stars),
                new NpgsqlParameter("@comment", rating.Comment ?? string.Empty),
                new NpgsqlParameter("@isConfirmed", rating.IsConfirmed),
                new NpgsqlParameter("@createdAt", rating.CreatedAt)
            );

            if (result != null && int.TryParse(result.ToString(), out int id))
            {
                rating.Id = id;
                return true;
            }

            return false;
        }

        public static Rating? GetById(int id)
        {
            string sql = @"
                SELECT id, media_id, user_id, stars, comment, is_confirmed, like_count, created_at, updated_at
                FROM ratings
                WHERE id = @id";

            using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@id", id));

            if (reader.Read())
            {
                return MapReaderToRating(reader);
            }

            return null;
        }

        public static Rating? GetByUserAndMedia(int userId, int mediaId)
        {
            string sql = @"
                SELECT id, media_id, user_id, stars, comment, is_confirmed, like_count, created_at, updated_at
                FROM ratings
                WHERE user_id = @userId AND media_id = @mediaId";

            using var reader = DB.ExecuteReader(sql,
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@mediaId", mediaId));

            if (reader.Read())
            {
                return MapReaderToRating(reader);
            }

            return null;
        }

        public static List<Rating> GetByMedia(int mediaId, bool confirmedOnly = false)
        {
            string sql = @"
                SELECT id, media_id, user_id, stars, comment, is_confirmed, like_count, created_at, updated_at
                FROM ratings
                WHERE media_id = @mediaId";

            if (confirmedOnly)
                sql += " AND is_confirmed = true";

            sql += " ORDER BY created_at DESC";

            var ratings = new List<Rating>();

            using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@mediaId", mediaId));
            while (reader.Read())
            {
                ratings.Add(MapReaderToRating(reader));
            }

            return ratings;
        }

        public static List<Rating> GetByUser(int userId)
        {
            string sql = @"
                SELECT id, media_id, user_id, stars, comment, is_confirmed, like_count, created_at, updated_at
                FROM ratings
                WHERE user_id = @userId
                ORDER BY created_at DESC";

            var ratings = new List<Rating>();

            using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@userId", userId));
            while (reader.Read())
            {
                ratings.Add(MapReaderToRating(reader));
            }

            return ratings;
        }

        public static bool UpdateRating(Rating rating)
        {
            string sql = @"
                UPDATE ratings
                SET stars = @stars, 
                    comment = @comment, 
                    is_confirmed = @isConfirmed,
                    updated_at = @updatedAt
                WHERE id = @id";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@stars", rating.Stars),
                new NpgsqlParameter("@comment", rating.Comment ?? string.Empty),
                new NpgsqlParameter("@isConfirmed", rating.IsConfirmed),
                new NpgsqlParameter("@updatedAt", DateTime.UtcNow),
                new NpgsqlParameter("@id", rating.Id)
            );

            return rows > 0;
        }

        public static bool DeleteRating(int id)
        {
            string sql = "DELETE FROM ratings WHERE id = @id";
            int rows = DB.ExecuteNonQuery(sql, new NpgsqlParameter("@id", id));
            return rows > 0;
        }

        public static bool ConfirmRating(int id)
        {
            string sql = @"
                UPDATE ratings
                SET is_confirmed = true, updated_at = @updatedAt
                WHERE id = @id";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@updatedAt", DateTime.UtcNow),
                new NpgsqlParameter("@id", id)
            );

            return rows > 0;
        }

        public static double GetAverageRating(int mediaId)
        {
            string sql = @"
                SELECT AVG(stars)::float
                FROM ratings
                WHERE media_id = @mediaId AND is_confirmed = true";

            object? result = DB.ExecuteScalar(sql, new NpgsqlParameter("@mediaId", mediaId));

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }

            return 0.0;
        }

        public static int GetRatingCount(int mediaId)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM ratings
                WHERE media_id = @mediaId AND is_confirmed = true";

            object? result = DB.ExecuteScalar(sql, new NpgsqlParameter("@mediaId", mediaId));

            if (result != null)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        public static bool LikeRating(int ratingId, int userId)
        {
            string sql = @"
                INSERT INTO rating_likes (rating_id, user_id, created_at)
                VALUES (@ratingId, @userId, @createdAt)
                ON CONFLICT (rating_id, user_id) DO NOTHING";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@ratingId", ratingId),
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@createdAt", DateTime.UtcNow)
            );

            if (rows > 0)
            {
                UpdateLikeCount(ratingId);
                return true;
            }

            return false;
        }

        public static bool UnlikeRating(int ratingId, int userId)
        {
            string sql = @"
                DELETE FROM rating_likes
                WHERE rating_id = @ratingId AND user_id = @userId";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@ratingId", ratingId),
                new NpgsqlParameter("@userId", userId)
            );

            if (rows > 0)
            {
                UpdateLikeCount(ratingId);
                return true;
            }

            return false;
        }

        private static void UpdateLikeCount(int ratingId)
        {
            string sql = @"
                UPDATE ratings
                SET like_count = (
                    SELECT COUNT(*) FROM rating_likes WHERE rating_id = @ratingId
                )
                WHERE id = @ratingId";

            DB.ExecuteNonQuery(sql, new NpgsqlParameter("@ratingId", ratingId));
        }

        private static Rating MapReaderToRating(NpgsqlDataReader reader)
        {
            return new Rating
            {
                Id = reader.GetInt32(0),
                MediaId = reader.GetInt32(1),
                UserId = reader.GetInt32(2),
                Stars = reader.GetInt32(3),
                Comment = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                IsConfirmed = reader.GetBoolean(5),
                LikeCount = reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
            };
        }
    }
}