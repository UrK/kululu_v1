using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dror.Common.Data.Contracts;

namespace Kululu.Entities
{
    public class Song : IEntity, IActivity
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string VideoID { get; set; }
        public virtual string ArtistName { get; set; }
        public virtual double? Duration { get; set; }

        public virtual DateTime LastUpdated { get; set; }

        private readonly string verb = "Added Song";
        public virtual string Verb { get { return verb; } set { ; } }

        public virtual string ImageUrl { get; set; }

        public Song()
        {
            this.LastUpdated = DateTime.Now;
        }

        public virtual bool IsOwner(object id)
        {
            return false;
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