using dotnet_restful_media_review_server.System;
using Npgsql;

namespace dotnet_restful_media_review_server.Database
{
    public static class MediaRepository
    {
        public static bool CreateMedia(MediaEntry media)
        {
            string sql = @"
                INSERT INTO media_entries 
                (title, media_type, genre, release_year, age_restriction, description, creator_user_id, created_at)
                VALUES (@title, @mediaType, @genre, @releaseYear, @ageRestriction, @description, @creatorUserId, @createdAt)
                RETURNING id";

            object? result = DB.ExecuteScalar(sql,
                new NpgsqlParameter("@title", media.Title),
                new NpgsqlParameter("@mediaType", media.MediaType),
                new NpgsqlParameter("@genre", media.Genre),
                new NpgsqlParameter("@releaseYear", media.ReleaseYear),
                new NpgsqlParameter("@ageRestriction", media.AgeRestriction),
                new NpgsqlParameter("@description", media.Description),
                new NpgsqlParameter("@creatorUserId", media.CreatorUserId),
                new NpgsqlParameter("@createdAt", media.CreatedAt)
            );

            if (result != null && int.TryParse(result.ToString(), out int id))
            {
                media.Id = id;
                return true;
            }

            return false;
        }

        public static MediaEntry? GetById(int id)
        {
            string sql = @"
                SELECT id, title, media_type, genre, release_year, age_restriction, 
                       description, creator_user_id, created_at, updated_at
                FROM media_entries
                WHERE id = @id";

            using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@id", id));

            if (reader.Read())
            {
                return MapReaderToMedia(reader);
            }

            return null;
        }

        public static List<MediaEntry> GetAll()
        {
            string sql = @"
                SELECT id, title, media_type, genre, release_year, age_restriction, 
                       description, creator_user_id, created_at, updated_at
                FROM media_entries
                ORDER BY created_at DESC";

            var mediaList = new List<MediaEntry>();

            using var reader = DB.ExecuteReader(sql);
            while (reader.Read())
            {
                mediaList.Add(MapReaderToMedia(reader));
            }

            return mediaList;
        }

        public static List<MediaEntry> GetByCreator(int userId)
        {
            string sql = @"
                SELECT id, title, media_type, genre, release_year, age_restriction, 
                       description, creator_user_id, created_at, updated_at
                FROM media_entries
                WHERE creator_user_id = @userId
                ORDER BY created_at DESC";

            var mediaList = new List<MediaEntry>();

            using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@userId", userId));
            while (reader.Read())
            {
                mediaList.Add(MapReaderToMedia(reader));
            }

            return mediaList;
        }

        public static bool UpdateMedia(MediaEntry media)
        {
            string sql = @"
                UPDATE media_entries
                SET title = @title, 
                    media_type = @mediaType, 
                    genre = @genre, 
                    release_year = @releaseYear, 
                    age_restriction = @ageRestriction, 
                    description = @description,
                    updated_at = @updatedAt
                WHERE id = @id";

            int rows = DB.ExecuteNonQuery(sql,
                new NpgsqlParameter("@title", media.Title),
                new NpgsqlParameter("@mediaType", media.MediaType),
                new NpgsqlParameter("@genre", media.Genre),
                new NpgsqlParameter("@releaseYear", media.ReleaseYear),
                new NpgsqlParameter("@ageRestriction", media.AgeRestriction),
                new NpgsqlParameter("@description", media.Description),
                new NpgsqlParameter("@updatedAt", DateTime.UtcNow),
                new NpgsqlParameter("@id", media.Id)
            );

            return rows > 0;
        }

        public static bool DeleteMedia(int id)
        {
            string sql = "DELETE FROM media_entries WHERE id = @id";

            int rows = DB.ExecuteNonQuery(sql, new NpgsqlParameter("@id", id));

            return rows > 0;
        }

        private static MediaEntry MapReaderToMedia(NpgsqlDataReader reader)
        {
            return new MediaEntry
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
            };
        }
    }
}