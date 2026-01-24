using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class ProfileHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/profile"))
                return;

            // GET /profile - Get current user's profile and statistics
            if (e.Path == "/profile" && e.Method == HttpMethod.Get)
            {
                HandleGetProfile(e);
                return;
            }

            // GET /profile/ratings - Get user's rating history
            if (e.Path == "/profile/ratings" && e.Method == HttpMethod.Get)
            {
                HandleGetRatingHistory(e);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid profile endpoint"
            });
            e.Responded = true;
        }

        private void HandleGetProfile(HttpRestEventArgs e)
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

                // Get statistics
                var userRatings = RatingRepository.GetByUser(user.Id);
                var userMedia = MediaRepository.GetByCreator(user.Id);
                int favoriteCount = FavoriteRepository.GetFavoriteCount(user.Id);

                // Calculate average stars given
                double avgStarsGiven = 0;
                if (userRatings.Count > 0)
                {
                    avgStarsGiven = userRatings.Average(r => r.Stars);
                }

                // Calculate total likes received on user's ratings
                int totalLikesReceived = userRatings.Sum(r => r.LikeCount);

                var response = new JsonObject
                {
                    ["user"] = new JsonObject
                    {
                        ["id"] = user.Id,
                        ["username"] = user.UserName,
                        ["fullName"] = user.FullName,
                        ["email"] = user.Email,
                        ["createdAt"] = user.CreatedAt.ToString("o")
                    },
                    ["statistics"] = new JsonObject
                    {
                        ["totalRatings"] = userRatings.Count,
                        ["totalMediaCreated"] = userMedia.Count,
                        ["totalFavorites"] = favoriteCount,
                        ["averageStarsGiven"] = Math.Round(avgStarsGiven, 2),
                        ["totalLikesReceived"] = totalLikesReceived
                    }
                };

                e.Respond(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve profile"
                });
                Console.WriteLine($"Error in GetProfile: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleGetRatingHistory(HttpRestEventArgs e)
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

                var ratings = RatingRepository.GetByUser(user.Id);
                var arr = new JsonArray();

                foreach (var rating in ratings)
                {
                    var media = MediaRepository.GetById(rating.MediaId);
                    
                    arr.Add(new JsonObject
                    {
                        ["ratingId"] = rating.Id,
                        ["stars"] = rating.Stars,
                        ["comment"] = rating.Comment,
                        ["isConfirmed"] = rating.IsConfirmed,
                        ["likeCount"] = rating.LikeCount,
                        ["createdAt"] = rating.CreatedAt.ToString("o"),
                        ["media"] = new JsonObject
                        {
                            ["id"] = media?.Id ?? 0,
                            ["title"] = media?.Title ?? "Unknown",
                            ["mediaType"] = media?.MediaType ?? "Unknown"
                        }
                    });
                }

                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve rating history"
                });
                Console.WriteLine($"Error in GetRatingHistory: {ex.Message}");
            }

            e.Responded = true;
        }
    }
}