namespace dotnet_restful_media_review_server.System
{
    public sealed class Rating
    {
        public int Id { get; set; }
        public int MediaId { get; set; }
        public int UserId { get; set; }
        public int Stars { get; set; } // 1-5
        public string Comment { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; } // Comments require confirmation before public visibility
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}