using dotnet_restful_media_review_server.System;

namespace dotnet_restful_media_review_server.System
{
    public sealed class Rating : Atom
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string MediaId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public int Stars { get; set; }
        public string Comment { get; set; } = string.Empty;

        public override void Save()
        {
            _EndEdit();
        }

        public override void Delete()
        {
            _EndEdit();
        }

        public override void Refresh()
        {
            // Later used for DB load
        }
    }
}
