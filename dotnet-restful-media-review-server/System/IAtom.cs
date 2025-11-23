namespace dotnet_restful_media_review_server.System
{
    public interface IAtom
    {
        public void BeginEdit(Session session);

        public void Save();

        public void Delete();

        public void Refresh();
    }
}
