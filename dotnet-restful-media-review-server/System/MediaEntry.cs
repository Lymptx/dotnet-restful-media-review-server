namespace dotnet_restful_media_review_server.System
{
    public sealed class MediaEntry
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty; // movie, tv_show, game, book
        public string Genre { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int AgeRestriction { get; set; } // 0, 6, 12, 16, 18
        public string Description { get; set; } = string.Empty;
        public int CreatorUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}