namespace dotnet_restful_media_review_server.Database
{
    public class UserRecord
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}

