using dotnet_restful_media_review_server.Server;
using dotnet_restful_media_review_server.System;
using dotnet_restful_media_review_server.Database;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace dotnet_restful_media_review_server.Handlers
{
    public sealed class MediaHandler : Handler, IHandler
    {
        public override void Handle(HttpRestEventArgs e)
        {
            if (!e.Path.StartsWith("/media"))
                return;

            // GET /media - List all media
            if (e.Path == "/media" && e.Method == HttpMethod.Get)
            {
                HandleGetAllMedia(e);
                return;
            }

            // POST /media - Create new media (requires auth)
            if (e.Path == "/media" && e.Method == HttpMethod.Post)
            {
                HandleCreateMedia(e);
                return;
            }

            // GET /media/{id} - Get specific media
            var getMatch = Regex.Match(e.Path, @"^/media/(\d+)$");
            if (getMatch.Success && e.Method == HttpMethod.Get)
            {
                int id = int.Parse(getMatch.Groups[1].Value);
                HandleGetMediaById(e, id);
                return;
            }

            // PUT /media/{id} - Update media (requires auth + ownership)
            var putMatch = Regex.Match(e.Path, @"^/media/(\d+)$");
            if (putMatch.Success && e.Method == HttpMethod.Put)
            {
                int id = int.Parse(putMatch.Groups[1].Value);
                HandleUpdateMedia(e, id);
                return;
            }

            // DELETE /media/{id} - Delete media (requires auth + ownership)
            var deleteMatch = Regex.Match(e.Path, @"^/media/(\d+)$");
            if (deleteMatch.Success && e.Method == HttpMethod.Delete)
            {
                int id = int.Parse(deleteMatch.Groups[1].Value);
                HandleDeleteMedia(e, id);
                return;
            }

            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid media endpoint"
            });
            e.Responded = true;
        }

        private void HandleGetAllMedia(HttpRestEventArgs e)
        {
            try
            {
                var mediaList = MediaRepository.GetAll();
                var arr = new JsonArray();

                foreach (var media in mediaList)
                {
                    arr.Add(MediaToJson(media));
                }

                e.Respond(HttpStatusCode.OK, arr);
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve media entries"
                });
                Console.WriteLine($"Error in GetAllMedia: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleGetMediaById(HttpRestEventArgs e, int id)
        {
            try
            {
                var media = MediaRepository.GetById(id);

                if (media == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Media entry not found"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.OK, MediaToJson(media));
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Failed to retrieve media entry"
                });
                Console.WriteLine($"Error in GetMediaById: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleCreateMedia(HttpRestEventArgs e)
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

                string title = e.Content?["title"]?.GetValue<string>() ?? string.Empty;
                string mediaType = e.Content?["mediaType"]?.GetValue<string>() ?? string.Empty;
                string genre = e.Content?["genre"]?.GetValue<string>() ?? string.Empty;
                int releaseYear = e.Content?["releaseYear"]?.GetValue<int>() ?? 0;
                int ageRestriction = e.Content?["ageRestriction"]?.GetValue<int>() ?? 0;
                string description = e.Content?["description"]?.GetValue<string>() ?? string.Empty;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(title))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Title is required"
                    });
                    e.Responded = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(mediaType))
                {
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Media type is required"
                    });
                    e.Responded = true;
                    return;
                }

                // Get creator user ID from database
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

                MediaEntry media = new()
                {
                    Title = title,
                    MediaType = mediaType,
                    Genre = genre,
                    ReleaseYear = releaseYear,
                    AgeRestriction = ageRestriction,
                    Description = description,
                    CreatorUserId = user.Id
                };

                bool created = MediaRepository.CreateMedia(media);

                if (created)
                {
                    e.Respond(HttpStatusCode.Created, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Media entry created successfully",
                        ["id"] = media.Id
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Failed to create media entry"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while creating the media entry"
                });
                Console.WriteLine($"Error in CreateMedia: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleUpdateMedia(HttpRestEventArgs e, int id)
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

                // Get existing media
                var existingMedia = MediaRepository.GetById(id);
                if (existingMedia == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Media entry not found"
                    });
                    e.Responded = true;
                    return;
                }

                // Get user ID
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
                if (existingMedia.CreatorUserId != user.Id && !e.Session.IsAdmin)
                {
                    e.Respond(HttpStatusCode.Forbidden, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "You can only edit your own media entries"
                    });
                    e.Responded = true;
                    return;
                }

                // Update fields
                existingMedia.Title = e.Content?["title"]?.GetValue<string>() ?? existingMedia.Title;
                existingMedia.MediaType = e.Content?["mediaType"]?.GetValue<string>() ?? existingMedia.MediaType;
                existingMedia.Genre = e.Content?["genre"]?.GetValue<string>() ?? existingMedia.Genre;
                existingMedia.ReleaseYear = e.Content?["releaseYear"]?.GetValue<int>() ?? existingMedia.ReleaseYear;
                existingMedia.AgeRestriction = e.Content?["ageRestriction"]?.GetValue<int>() ?? existingMedia.AgeRestriction;
                existingMedia.Description = e.Content?["description"]?.GetValue<string>() ?? existingMedia.Description;
                existingMedia.UpdatedAt = DateTime.UtcNow;

                bool updated = MediaRepository.UpdateMedia(existingMedia);

                if (updated)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Media entry updated successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Failed to update media entry"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while updating the media entry"
                });
                Console.WriteLine($"Error in UpdateMedia: {ex.Message}");
            }

            e.Responded = true;
        }

        private void HandleDeleteMedia(HttpRestEventArgs e, int id)
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

                // Get existing media
                var existingMedia = MediaRepository.GetById(id);
                if (existingMedia == null)
                {
                    e.Respond(HttpStatusCode.NotFound, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Media entry not found"
                    });
                    e.Responded = true;
                    return;
                }

                // Get user ID
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
                if (existingMedia.CreatorUserId != user.Id && !e.Session.IsAdmin)
                {
                    e.Respond(HttpStatusCode.Forbidden, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "You can only delete your own media entries"
                    });
                    e.Responded = true;
                    return;
                }

                bool deleted = MediaRepository.DeleteMedia(id);

                if (deleted)
                {
                    e.Respond(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Media entry deleted successfully"
                    });
                }
                else
                {
                    e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                    {
                        ["success"] = false,
                        ["reason"] = "Failed to delete media entry"
                    });
                }
            }
            catch (Exception ex)
            {
                e.Respond(HttpStatusCode.InternalServerError, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "An error occurred while deleting the media entry"
                });
                Console.WriteLine($"Error in DeleteMedia: {ex.Message}");
            }

            e.Responded = true;
        }

        private JsonObject MediaToJson(MediaEntry media)
        {
            return new JsonObject
            {
                ["id"] = media.Id,
                ["title"] = media.Title,
                ["mediaType"] = media.MediaType,
                ["genre"] = media.Genre,
                ["releaseYear"] = media.ReleaseYear,
                ["ageRestriction"] = media.AgeRestriction,
                ["description"] = media.Description,
                ["creatorUserId"] = media.CreatorUserId,
                ["createdAt"] = media.CreatedAt.ToString("o"),
                ["updatedAt"] = media.UpdatedAt?.ToString("o")
            };
        }
    }
}