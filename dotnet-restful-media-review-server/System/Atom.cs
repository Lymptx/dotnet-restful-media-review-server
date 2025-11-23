namespace dotnet_restful_media_review_server.System
{
    //base implementation for data objects
    public abstract class Atom : IAtom
    {
        protected Session? _EditingSession = null;

        //make sure user  is logged in
        protected void _VerifySession(Session? session = null)
        {
            if (session is not null) { _EditingSession = session; }
            if (_EditingSession is null || !_EditingSession.Valid) { throw new UnauthorizedAccessException("Invalid session."); }
        }

        protected void _EndEdit()
        {
            _EditingSession = null;
        }

        //only admin allowed
        protected void _EnsureAdmin()
        {
            _VerifySession();
            if (!_EditingSession!.IsAdmin) { throw new UnauthorizedAccessException("Admin privileges required."); }
        }

        //admin and owner allowed
        protected void _EnsureAdminOrOwner(string owner)
        {
            _VerifySession();
            if (!(_EditingSession!.IsAdmin || (_EditingSession.UserName == owner)))
            {
                throw new UnauthorizedAccessException("Admin or owner privileges required.");
            }
        }


        public virtual void BeginEdit(Session session)
        {
            _VerifySession(session);
        }

        public abstract void Save();

        public abstract void Delete();

        public abstract void Refresh();
    }
}
