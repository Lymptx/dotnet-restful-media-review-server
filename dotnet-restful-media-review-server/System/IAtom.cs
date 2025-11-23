using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
