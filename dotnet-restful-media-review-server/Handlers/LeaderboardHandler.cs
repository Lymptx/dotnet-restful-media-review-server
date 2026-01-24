using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;
using Npgsql;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class LeaderboardHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/leaderboard"))
                return;

            // GET /leaderboard/media - Top rated media
            if (e.Path == "/leaderboard/media" && e.Method == HttpMethod.Get)
            {
                HandleTopMedia(e);
                return;
            }

            // GET /leaderboard/users - Most active users
            if (e.Path == "/leaderboard/users" && e.Method == HttpMethod.Get)
            {
                HandleTopUsers(e);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid leaderboard endpoint"
            });
            e.Responded = true;
        }

        private void HandleTopMedia(HttpRestEventArgs e)
        {
            try
            {
                string limitStr = e.Context.Request.QueryString["limit"] ?? "10";
                int limit = int.TryParse(limitStr, out int l) ? l : 10;
                limit = Math.Min(limit, 100); // Max 100

                string sql = @"
                    SELECT m.id, m.title, m.media_type, m.genre, m.release_year,
                           AVG(r.stars)::float as avg_rating,
                           COUNT(r.id) as rating_count
                    FROM media_entries m
                    INNER JOIN ratings r ON m.id = r.media_id
                    WHERE r.is_confirmed = true
                    GROUP BY m.id, m.title, m.media_type, m.genre, m.release_year
                    HAVING COUNT(r.id) >= 3
                    ORDER BY avg_rating DESC, rating_count DESC
                    LIMIT @limit";

                var arr = new JsonArray();

                using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@limit", limit));
                int rank = 1;
                while (reader.Read())
                {
                    arr.Add(new JsonObject
                    {
                        ["rank"] = rank++,
                        ["mediaId"] = reader.GetInt32(0),
                        ["title"] = reader.GetString(1),
                        ["mediaType"] = reader.GetString(2),
                        ["genre"] = reader.GetString(3),
                        ["releaseYear"] = reader.GetInt32(4),
                        ["averageRating"] = Math.Round(reader.GetDouble(5), 2),
                        ["ratingCount"] = reader.GetInt32(6)
                    });
                }

                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve top media"
                });
                Console.WriteLine($"Error in TopMedia: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleTopUsers(HttpRestEventArgs e)
        {
            try
            {
                string limitStr = e.Context.Request.QueryString["limit"] ?? "10";
                int limit = int.TryParse(limitStr, out int l) ? l : 10;
                limit = Math.Min(limit, 100); // Max 100

                string sql = @"
                    SELECT u.id, u.username, u.full_name,
                           COUNT(DISTINCT r.id) as total_ratings,
                           COUNT(DISTINCT m.id) as total_media_created,
                           COALESCE(SUM(r.like_count), 0) as total_likes_received
                    FROM users u
                    LEFT JOIN ratings r ON u.id = r.user_id
                    LEFT JOIN media_entries m ON u.id = m.creator_user_id
                    GROUP BY u.id, u.username, u.full_name
                    HAVING COUNT(DISTINCT r.id) > 0 OR COUNT(DISTINCT m.id) > 0
                    ORDER BY (COUNT(DISTINCT r.id) + COUNT(DISTINCT m.id) * 2) DESC
                    LIMIT @limit";

                var arr = new JsonArray();

                using var reader = DB.ExecuteReader(sql, new NpgsqlParameter("@limit", limit));
                int rank = 1;
                while (reader.Read())
                {
                    arr.Add(new JsonObject
                    {
                        ["rank"] = rank++,
                        ["userId"] = reader.GetInt32(0),
                        ["username"] = reader.GetString(1),
                        ["fullName"] = reader.GetString(2),
                        ["totalRatings"] = reader.GetInt32(3),
                        ["totalMediaCreated"] = reader.GetInt32(4),
                        ["totalLikesReceived"] = Convert.ToInt32(reader.GetValue(5))
                    });
                }

                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve top users"
                });
                Console.WriteLine($"Error in TopUsers: {ex.Message}");
            }

            e.Responded = true;
        }
    }
}