using System;
using System.Collections.Generic;
using Dror.Common.Data.Contracts;
using Kululu.Entities.Common;

namespace Kululu.Entities
{
    public class FbUser : IEntity, IActivity
    {
        #region Ctors
        public FbUser()
        {
            Businesses = new List<LocalBusiness>();
            PlaylistsInfo = new List<UserPlaylistInfo>();
        }

        public FbUser(long userId) : this()
        {
            Id = userId;
        }
        
        #endregion

        #region Properties

        private const string VERB = "Joined";
        public virtual string Verb { get { return VERB; } set { } }

        public virtual Int64 Id { get; set;}

        public virtual string RelationshipStatus{get;set;}

        public virtual FbUser SignificantOther { get; set; }
       
        public virtual string Name {get;set;}

        public virtual string ProfileImageUrl{get;set;}

        public virtual string LinkToProfile{ get; set; }

        public virtual string Gender { get; set; }

        //public virtual DateTime Birthday { get; set; }

        public virtual DateTime JoinDate { get; set; }

        public virtual UserStatus Status { get; set; }

        public virtual string FullName { get; set; }
        
        public virtual string Email { get; set; }

        public virtual IList<LocalBusiness> Businesses { get; set; }

        public virtual IList<UserPlaylistInfo> PlaylistsInfo { get; set; }

        /// <summary>
        /// locale setting for current user
        /// </summary>
        public virtual string Locale { get; set; }

        #endregion

        public virtual bool IsOwner(object id)
        {
            long _id;
            if (long.TryParse(id.ToString(), out _id))
            {
                return this.Id == _id;
            }
            return false;
        }


        public virtual event RepositoryTransactionCompletin OnSaving;
        public virtual void RiseSaving()
        {
            /* if the full name of the user is not set, pull it from Facebook
             */
            if (String.IsNullOrEmpty(FullName))
            {
                try
                {
                    FullName =
                        Dror.Common.Utils.Facebook.GetFbUserFullName(Id);
                }
                catch { }
            }

            if (OnSaving != null)
            {
                OnSaving();
            }
        }
    }

    public class DistinctUser : IEqualityComparer<FbUser>
    {
        #region IEqualityComparer<FbUser> Members

        public bool Equals(FbUser x, FbUser y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(FbUser obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }

}