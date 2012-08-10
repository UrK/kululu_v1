using System;
using Dror.Common.Data.Contracts;
using Kululu.Entities.Common;

namespace Kululu.Entities
{
    public class RatingDetails : IEntity
    {
        public virtual long Id { get; set; }
        public virtual short Rating { get; set; }
        public virtual DateTime LastUpdated { get; set; }
        public virtual FbUser  VotingUser { get; set; }
        public virtual PlaylistSongRating PlaylistSongRating { get; set; }

        private readonly string verb = "Rated Song";
        public virtual string Verb { get { return verb; } set { ; } }

        public RatingDetails()
        {
            LastUpdated = DateTime.Now;
        }

        public RatingDetails(FbUser votingUser, short rating) : this()
        {
            VotingUser = votingUser;
            Rating = rating;
            OnSaving +=new RepositoryTransactionCompletin(RatingDetails_OnSaving);
        }

        public virtual bool IsOwner(object id)
        {
            long _id;
            if (long.TryParse(id.ToString(), out _id))
            {
                return this.VotingUser.Id == _id;
            }
            return false;
        }

        private void RatingDetails_OnSaving()
        {
            //VotingUser.RewardScore(ActionsScore.Vote);
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