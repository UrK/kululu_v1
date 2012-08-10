using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dror.Common.Data.Contracts;

namespace Kululu.Entities
{
    public class Visit : IEntity
    {
        public virtual long Id { get; set; }
        public virtual FbUser User { get; set; }
        public virtual Playlist Playlist { get; set; }
        public virtual DateTime VisitDate { get; set; }
        
        public virtual bool IsOwner(object Id)
        {
            return (User.Id == (long)Id);
        }

        public virtual event RepositoryTransactionCompletin OnSaving;

        public virtual void RiseSaving()
        {
            if (OnSaving != null)
            {
                OnSaving();
            }
        }
    }
}
