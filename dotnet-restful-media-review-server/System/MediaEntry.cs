using dotnet_restful_media_review_server.System;

namespace dotnet_restful_media_review_server.System
{
    public sealed class MediaEntry : Atom
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;  // movie, game, series
        public int ReleaseYear { get; set; }
        public string[] Genres { get; set; } = Array.Empty<string>();

        // The user who created the media
        public string Owner { get; set; } = string.Empty;


        public override void Save()
        {
            // Intermediate submission:
            // No DB logic yet
            _EndEdit();
        }

        public override void Delete()
        {
            _EndEdit();
        }

        public override void Refresh()
        {
            //todo or final handin
        }
    }
}
