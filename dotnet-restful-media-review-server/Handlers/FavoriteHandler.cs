using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class FavoriteHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/favorites"))
                return;

            // GET /favorites - Get user's favorites
            if (e.Path == "/favorites" && e.Method == HttpMethod.Get)
            {
                HandleGetFavorites(e);
                return;
            }

            // POST /favorites/{mediaId} - Add to favorites
            var addMatch = Regex.Match(e.Path, @"^/favorites/(\d+)$");
            if (addMatch.Success && e.Method == HttpMethod.Post)
            {
                int mediaId = int.Parse(addMatch.Groups[1].Value);
                HandleAddFavorite(e, mediaId);
                return;
            }

            // DELETE /favorites/{mediaId} - Remove from favorites
            var removeMatch = Regex.Match(e.Path, @"^/favorites/(\d+)$");
            if (removeMatch.Success && e.Method == HttpMethod.Delete)
            {
                int mediaId = int.Parse(removeMatch.Groups[1].Value);
                HandleRemoveFavorite(e, mediaId);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid favorites endpoint"
            });
            e.Responded = true;
        }

        private void HandleGetFavorites(HttpRestEventArgs e)
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

                var favorites = FavoriteRepository.GetUserFavorites(user.Id);
                var arr = new JsonArray();

                foreach (var media in favorites)
                {
                    arr.Add(new JsonObject
                    {
                        ["id"] = media.Id,
                        ["title"] = media.Title,
                        ["mediaType"] = media.MediaType,
                        ["genre"] = media.Genre,
                        ["releaseYear"] = media.ReleaseYear,
                        ["ageRestriction"] = media.AgeRestriction,
                        ["description"] = media.Description,
                        ["averageRating"] = RatingRepository.GetAverageRating(media.Id)
                    });
                }

                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve favorites"
                });
                Console.WriteLine($"Error in GetFavorites: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleAddFavorite(HttpRestEventArgs e, int mediaId)
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

                bool added = FavoriteRepository.AddFavorite(user.Id, mediaId);

                if (added)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Added to favorites"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Already in favorites"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to add to favorites"
                });
                Console.WriteLine($"Error in AddFavorite: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleRemoveFavorite(HttpRestEventArgs e, int mediaId)
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

                bool removed = FavoriteRepository.RemoveFavorite(user.Id, mediaId);

                if (removed)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Removed from favorites"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Not in favorites"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to remove from favorites"
                });
                Console.WriteLine($"Error in RemoveFavorite: {ex.Message}");
            }

            e.Responded = true;
        }
    }
}