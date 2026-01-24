using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class RatingHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/ratings"))
                return;

            // GET /ratings?mediaId=1 - Get ratings for a media entry
            if (e.Path == "/ratings" && e.Method == HttpMethod.Get)
            {
                HandleGetRatings(e);
                return;
            }

            // POST /ratings - Create/update a rating (requires auth)
            if (e.Path == "/ratings" && e.Method == HttpMethod.Post)
            {
                HandleCreateOrUpdateRating(e);
                return;
            }

            // DELETE /ratings/{id} - Delete a rating (requires auth)
            var deleteMatch = Regex.Match(e.Path, @"^/ratings/(\d+)$");
            if (deleteMatch.Success && e.Method == HttpMethod.Delete)
            {
                int id = int.Parse(deleteMatch.Groups[1].Value);
                HandleDeleteRating(e, id);
                return;
            }

            // POST /ratings/{id}/like - Like a rating (requires auth)
            var likeMatch = Regex.Match(e.Path, @"^/ratings/(\d+)/like$");
            if (likeMatch.Success && e.Method == HttpMethod.Post)
            {
                int id = int.Parse(likeMatch.Groups[1].Value);
                HandleLikeRating(e, id);
                return;
            }

            // DELETE /ratings/{id}/like - Unlike a rating (requires auth)
            var unlikeMatch = Regex.Match(e.Path, @"^/ratings/(\d+)/like$");
            if (unlikeMatch.Success && e.Method == HttpMethod.Delete)
            {
                int id = int.Parse(unlikeMatch.Groups[1].Value);
                HandleUnlikeRating(e, id);
                return;
            }

            // POST /ratings/{id}/confirm - Confirm a rating (admin only)
            var confirmMatch = Regex.Match(e.Path, @"^/ratings/(\d+)/confirm$");
            if (confirmMatch.Success && e.Method == HttpMethod.Post)
            {
                int id = int.Parse(confirmMatch.Groups[1].Value);
                HandleConfirmRating(e, id);
                return;
            }

            // GET /ratings/pending - List all ratings needing confirmation (admin only)
            if (e.Path == "/ratings/pending" && e.Method == HttpMethod.Get)
            {
                HandleGetPendingRatings(e);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid rating endpoint"
            });
            e.Responded = true;
        }

        private void HandleGetRatings(HttpRestEventArgs e)
        {
            try
            {
                string mediaIdStr = e.Context.Request.QueryString["mediaId"] ?? string.Empty;

                if (string.IsNullOrWhiteSpace(mediaIdStr) || !int.TryParse(mediaIdStr, out int mediaId))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "mediaId query parameter is required"
                    });
                    e.Responded = true;
                    return;
                }

                // Check if media exists
                var media = MediaRepository.GetById(mediaId);
                if (media == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Media entry not found"
                    });
                    e.Responded = true;
                    return;
                }

                // Only show confirmed ratings to public
                var ratings = RatingRepository.GetByMedia(mediaId, confirmedOnly: true);
                var arr = new JsonArray();

                foreach (var rating in ratings)
                {
                    arr.Add(RatingToJson(rating));
                }

                // Include average rating and count
                var response = new JsonObject
                {
                    ["mediaId"] = mediaId,
                    ["averageRating"] = RatingRepository.GetAverageRating(mediaId),
                    ["totalRatings"] = RatingRepository.GetRatingCount(mediaId),
                    ["ratings"] = arr
                };

                e.Respond(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve ratings"
                });
                Console.WriteLine($"Error in GetRatings: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleCreateOrUpdateRating(HttpRestEventArgs e)
        {
            try
            {
                // Require authentication
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

                int mediaId = e.Content?["mediaId"]?.GetValue<int>() ?? 0;
                int stars = e.Content?["stars"]?.GetValue<int>() ?? 0;
                string comment = e.Content?["comment"]?.GetValue<string>() ?? string.Empty;

                // Validate input
                if (mediaId <= 0)
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "mediaId is required"
                    });
                    e.Responded = true;
                    return;
                }

                if (stars < 1 || stars > 5)
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "stars must be between 1 and 5"
                    });
                    e.Responded = true;
                    return;
                }

                // Check if media exists
                var media = MediaRepository.GetById(mediaId);
                if (media == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Media entry not found"
                    });
                    e.Responded = true;
                    return;
                }

                // Get user
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

                // Check if rating already exists (one rating per user per media)
                var existingRating = RatingRepository.GetByUserAndMedia(user.Id, mediaId);

                if (existingRating != null)
                {
                    // Update existing rating
                    existingRating.Stars = stars;
                    existingRating.Comment = comment;
                    existingRating.IsConfirmed = false; // Re-confirmation needed if comment changed
                    existingRating.UpdatedAt = DateTime.UtcNow;

                    bool updated = RatingRepository.UpdateRating(existingRating);

                    if (updated)
                    {
                        e.Respond(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Rating updated successfully",
                            ["id"] = existingRating.Id
                        });
                    }
                    else
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = "Failed to update rating"
                        });
                    }
                }
                else
                {
                    // Create new rating
                    Rating rating = new()
                    {
                        MediaId = mediaId,
                        UserId = user.Id,
                        Stars = stars,
                        Comment = comment,
                        IsConfirmed = false // Comments require confirmation
                    };

                    bool created = RatingRepository.CreateRating(rating);

                    if (created)
                    {
                        e.Respond(HttpStatusCode.Created, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Rating created successfully. Comment pending confirmation.",
                            ["id"] = rating.Id
                        });
                    }
                    else
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                        {
                            ["success"] = false,
                            ["reason"] = "Failed to create rating"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = $"An error occurred while processing the rating: {ex.Message}"
                });
                Console.WriteLine($"Error in CreateOrUpdateRating: {ex}");
            }

            e.Responded = true;
        }

        private void HandleDeleteRating(HttpRestEventArgs e, int id)
        {
            try
            {
                // Require authentication
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

                // Get rating
                var rating = RatingRepository.GetById(id);
                if (rating == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Rating not found"
                    });
                    e.Responded = true;
                    return;
                }

                // Get user
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

                // Check ownership
                if (rating.UserId != user.Id && !e.Session.IsAdmin)
                {
                    e.Respond(HttpStatusCode.Forbidden, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "You can only delete your own ratings"
                    });
                    e.Responded = true;
                    return;
                }

                bool deleted = RatingRepository.DeleteRating(id);

                if (deleted)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Rating deleted successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Failed to delete rating"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while deleting the rating"
                });
                Console.WriteLine($"Error in DeleteRating: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleLikeRating(HttpRestEventArgs e, int ratingId)
        {
            try
            {
                // Require authentication
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

                // Check if rating exists
                var rating = RatingRepository.GetById(ratingId);
                if (rating == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Rating not found"
                    });
                    e.Responded = true;
                    return;
                }

                // Get user
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

                bool liked = RatingRepository.LikeRating(ratingId, user.Id);

                if (liked)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Rating liked successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Rating already liked"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while liking the rating"
                });
                Console.WriteLine($"Error in LikeRating: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleUnlikeRating(HttpRestEventArgs e, int ratingId)
        {
            try
            {
                // Require authentication
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

                // Get user
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

                bool unliked = RatingRepository.UnlikeRating(ratingId, user.Id);

                if (unliked)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Rating unliked successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Rating was not liked"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while unliking the rating"
                });
                Console.WriteLine($"Error in UnlikeRating: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleConfirmRating(HttpRestEventArgs e, int id)
        {
            try
            {
                // Require admin authentication
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
                if (!e.Session.IsAdmin)
                {
                    e.Respond(HttpStatusCode.Forbidden, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = $"Admin access required. Your username is '{e.Session.UserName}'. Only 'admin' is recognized as admin."
                    });
                    e.Responded = true;
                    return;
                }

                // Check if rating exists
                var rating = RatingRepository.GetById(id);
                if (rating == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Rating not found"
                    });
                    e.Responded = true;
                    return;
                }

                bool confirmed = RatingRepository.ConfirmRating(id);

                if (confirmed)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Rating confirmed successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Failed to confirm rating"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while confirming the rating"
                });
                Console.WriteLine($"Error in ConfirmRating: {ex.Message}");
            }

            e.Responded = true;
        }

        private JsonObject RatingToJson(Rating rating)
        {
            return new JsonObject
            {
                ["id"] = rating.Id,
                ["mediaId"] = rating.MediaId,
                ["userId"] = rating.UserId,
                ["stars"] = rating.Stars,
                ["comment"] = rating.Comment,
                ["isConfirmed"] = rating.IsConfirmed,
                ["likeCount"] = rating.LikeCount,
                ["createdAt"] = rating.CreatedAt.ToString("o"),
                ["updatedAt"] = rating.UpdatedAt?.ToString("o")
            };
        }

        private void HandleGetPendingRatings(HttpRestEventArgs e)
        {
            try
            {
                // Require admin authentication
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
                if (!e.Session.IsAdmin)
                {
                    e.Respond(HttpStatusCode.Forbidden, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = $"Admin access required. Your username is '{e.Session.UserName}'. Only 'admin' is recognized as admin."
                    });
                    e.Responded = true;
                    return;
                }

                var pendingRatings = RatingRepository.GetPendingRatings();
                var arr = new JsonArray();
                foreach (var rating in pendingRatings)
                {
                    arr.Add(RatingToJson(rating));
                }
                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve pending ratings"
                });
                Console.WriteLine($"Error in GetPendingRatings: {ex.Message}");
            }
            e.Responded = true;
        }
    }
}