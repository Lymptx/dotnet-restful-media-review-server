using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;
using Npgsql;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class RecommendationHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/recommendations"))
                return;

            // GET /recommendations - Get personalized recommendations
            if (e.Path == "/recommendations" && e.Method == HttpMethod.Get)
            {
                HandleGetRecommendations(e);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid recommendations endpoint"
            });
            e.Responded = true;
        }

        private void HandleGetRecommendations(HttpRestEventArgs e)
        {
            try
            {
                if (e.Session == null)
                {
                    e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Authentication required"
                    });
                    e.Responded = true;
                    return;
                }

                var user = UserRepository.GetByUsername(e.Session.UserName);
                if (user == null)
                {
                    e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "User not found"
                    });
                    e.Responded = true;
                    return;
                }

                string limitStr = e.Context.Request.QueryString["limit"] ?? "10";
                int limit = int.TryParse(limitStr, out int l) ? l : 10;
                limit = Math.Min(limit, 50);

                // Recommendation algorithm based on:
                // 1. Genres/types the user has rated highly (4-5 stars)
                // 2. Media not yet rated by the user
                // 3. Popular media (high average rating)
                string sql = @"
                    WITH user_preferences AS (
                        SELECT m.genre, m.media_type
                        FROM ratings r
                        INNER JOIN media_entries m ON r.media_id = m.id
                        WHERE r.user_id = @userId AND r.stars >= 4
                        GROUP BY m.genre, m.media_type
                    ),
                    media_scores AS (
                        SELECT m.id, m.title, m.media_type, m.genre, m.release_year,
                               AVG(r.stars)::float as avg_rating,
                               COUNT(r.id) as rating_count,
                               CASE 
                                   WHEN EXISTS (
                                       SELECT 1 FROM user_preferences up 
                                       WHERE up.genre = m.genre OR up.media_type = m.media_type
                                   ) THEN 2.0
                                   ELSE 1.0
                               END as preference_multiplier
                        FROM media_entries m
                        LEFT JOIN ratings r ON m.id = r.media_id AND r.is_confirmed = true
                        WHERE m.id NOT IN (
                            SELECT media_id FROM ratings WHERE user_id = @userId
                        )
                        GROUP BY m.id, m.title, m.media_type, m.genre, m.release_year
                        HAVING COUNT(r.id) >= 1
                    )
                    SELECT id, title, media_type, genre, release_year, avg_rating, rating_count,
                           (COALESCE(avg_rating, 0) * preference_multiplier + rating_count * 0.1) as score
                    FROM media_scores
                    ORDER BY score DESC, rating_count DESC
                    LIMIT @limit";

                var arr = new JsonArray();

                using var reader = DB.ExecuteReader(sql,
                    new NpgsqlParameter("@userId", user.Id),
                    new NpgsqlParameter("@limit", limit));

                while (reader.Read())
                {
                    arr.Add(new JsonObject
                    {
                        ["mediaId"] = reader.GetInt32(0),
                        ["title"] = reader.GetString(1),
                        ["mediaType"] = reader.GetString(2),
                        ["genre"] = reader.GetString(3),
                        ["releaseYear"] = reader.GetInt32(4),
                        ["averageRating"] = reader.IsDBNull(5) ? 0.0 : Math.Round(reader.GetDouble(5), 2),
                        ["ratingCount"] = reader.GetInt32(6),
                        ["recommendationScore"] = Math.Round(reader.GetDouble(7), 2)
                    });
                }

                if (arr.Count == 0)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["message"] = "Not enough data for recommendations. Try rating some media first!",
                        ["recommendations"] = arr
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["message"] = "Recommendations based on your rating history",
                        ["recommendations"] = arr
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to generate recommendations"
                });
                Console.WriteLine($"Error in GetRecommendations: {ex.Message}");
            }

            e.Responded = true;
        }
    }
}