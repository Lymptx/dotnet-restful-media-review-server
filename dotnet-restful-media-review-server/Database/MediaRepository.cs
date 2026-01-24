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

        public static List<MediaEntry> Search(string? title, string? mediaType, string? genre,
            int? minYear, int? maxYear, int? maxAgeRestriction, double? minRating)
        {
            var sql = @"
                SELECT DISTINCT m.id, m.title, m.media_type, m.genre, m.release_year, 
                       m.age_restriction, m.description, m.creator_user_id, 
                       m.created_at, m.updated_at
                FROM media_entries m";

            bool needsRatingJoin = minRating.HasValue && minRating.Value > 0;
            if (needsRatingJoin)
            {
                sql += @"
                LEFT JOIN (
                    SELECT media_id, AVG(stars)::float as avg_rating
                    FROM ratings
                    WHERE is_confirmed = true
                    GROUP BY media_id
                ) r ON m.id = r.media_id";
            }

            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrWhiteSpace(title))
            {
                conditions.Add("LOWER(m.title) LIKE LOWER(@title)");
                parameters.Add(new NpgsqlParameter("@title", $"%{title}%"));
            }

            if (!string.IsNullOrWhiteSpace(mediaType))
            {
                conditions.Add("LOWER(m.media_type) = LOWER(@mediaType)");
                parameters.Add(new NpgsqlParameter("@mediaType", mediaType));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                conditions.Add("LOWER(m.genre) LIKE LOWER(@genre)");
                parameters.Add(new NpgsqlParameter("@genre", $"%{genre}%"));
            }

            if (minYear.HasValue)
            {
                conditions.Add("m.release_year >= @minYear");
                parameters.Add(new NpgsqlParameter("@minYear", minYear.Value));
            }

            if (maxYear.HasValue)
            {
                conditions.Add("m.release_year <= @maxYear");
                parameters.Add(new NpgsqlParameter("@maxYear", maxYear.Value));
            }

            if (maxAgeRestriction.HasValue)
            {
                conditions.Add("m.age_restriction <= @maxAgeRestriction");
                parameters.Add(new NpgsqlParameter("@maxAgeRestriction", maxAgeRestriction.Value));
            }

            if (needsRatingJoin)
            {
                conditions.Add("(r.avg_rating >= @minRating OR r.avg_rating IS NULL)");
                parameters.Add(new NpgsqlParameter("@minRating", minRating.Value));
            }

            if (conditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }

            sql += " ORDER BY m.created_at DESC";

            var mediaList = new List<MediaEntry>();

            using var reader = DB.ExecuteReader(sql, parameters.ToArray());
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