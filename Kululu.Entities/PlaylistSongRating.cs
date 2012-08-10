using System;
using System.Collections.Generic;
using System.Linq;
using Kululu.Entities.Common;

namespace Kululu.Entities
{
    public enum RatingStatus
    {
        NothingNew,
        Error,
        AddedNewEmptyRating,
        AddedNewRating,
        AddedRating,
        RemovedRating
    }

    public enum Origin
    {
        App,
        Facebook
    }

    public class PlaylistSongRating : AggregatedRating
    {
        #region Properties
        
        public override long Id { get; set; }
        
        public virtual Origin Origin { get; set; }

        public virtual Song Song { get; set; }
        public virtual Playlist Playlist { get; set; }

        /// <summary>
        /// date/time of addition of songs to current playlist
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// This is different from CreationTime for content we retrieve from the facebook wall.
        /// We take the date it was added to the wall, not the date it was saved to our DB
        /// </summary>
        public virtual DateTime FacebookAddedDate { get; set; }
        /// <summary>
        /// who added the song to current playlist?
        /// </summary>
        public virtual FbUser Creator { get; set; }
        public virtual int NumOfComments { get; set; }
        
        public virtual IList<RatingDetails> Details { get; set; }

        #region Fb Settings
        public virtual string FBPostId { get; set; }
        public virtual string FBMessage { get; set; }
        public virtual string FBDescription { get; set; }
        #endregion

        public virtual bool IsAddedByAdmin { get; set; }
        public virtual short AdminRating { get; set; }

        #endregion Properties

        #region Ctors
        public PlaylistSongRating()
        {
            Details = new List<RatingDetails>();
            CreationTime = DateTime.Now;
            FacebookAddedDate = DateTime.Now;
            Origin = Entities.Origin.App;
        }

        /// <summary>
        /// create initial playlist rating
        /// </summary>
        ///
        /// <param name="playlist">
        /// rating to attach this rating to
        /// </param>
        ///
        /// <param name="song">
        /// song being rated
        /// </param>
        ///
        /// <param name="creator">
        /// initial creator of the rating
        /// </param>
        public PlaylistSongRating(Playlist playlist, Song song, FbUser creator)
            : this()
        {
            Song = song;
            Playlist = playlist;
            Creator = creator;
        }
        #endregion Ctors

        protected internal virtual RatingType[] GetUsersRatings()
        {
            var ratings = Details.Select(rating => rating.Rating).ToArray();
            //converting the array of returned shorts to an array of RatingType
            return Array.ConvertAll(ratings, rating => (RatingType)rating);
        }

        protected internal virtual RatingType GetCurrentUserRating(long currentUserId)
        {
            var ratings = Details.Where(rating => rating.VotingUser.Id == currentUserId)
                                           .Select(rating => rating.Rating);

            var currentRating  = ratings.LastOrDefault();
            return (RatingType)currentRating;
        }

        internal static bool IsRatingValueInvalid(int rating)
        {
            return (rating > 1 || rating < -1);
        }

        public virtual void SetAdminRating(short? oldRating, short newRating, bool isNewRating)
        {
            if (isNewRating)
            {
                base.AddAggregatedRating(newRating);
            }
            else if(oldRating.HasValue)
            {
                base.UpdatedAggregatedRating(oldRating.Value, newRating);
            }
        }

        /// <summary>
        /// adds rating to the song
        /// </summary>
        ///
        /// <param name="user">
        /// user adding current rating
        /// </param>
        ///
        /// <param name="rating">
        /// value of the rating added
        /// </param>
        ///
        /// <returns>
        /// A boolean indicating if rating was in the right input range
        /// (between -1 to 1)
        /// </returns>
        ///
        /// <remarks>
        /// if the user already has the rating for the song in specified
        /// playlist, update the rating. If the previous rating had value and
        /// the current one does not (rating = 0), decrease number of points
        /// of the user.
        /// if previous rating has been 0, and current one is not, add points
        /// to the user.
        /// if this is the first rating of the user, increase number of points
        /// for the user.
        /// 
        /// Note: Do not use this method for page \ admin rating
        /// </remarks>
        public virtual RatingStatus AddRating(FbUser user, short rating)
        {
            if (IsRatingValueInvalid(rating))
            {
                return RatingStatus.Error;
            }

            var songRating = Details.FirstOrDefault(lookupSongRating => lookupSongRating.VotingUser.Id == user.Id);
            if (songRating == null)
            {
                InsertNewSongRating(user, rating);
                return RatingStatus.AddedNewRating;
            }
            else
            {
                RatingStatus ratingStatus = RatingStatus.NothingNew;
                if (songRating.Rating != 0 && rating == 0)
                {
                    ratingStatus = RatingStatus.RemovedRating;
                }
                else if (songRating.Rating == 0 && rating != 0)
                {
                    ratingStatus= RatingStatus.AddedRating;
                }
                UpdatePlaylistSongRating(songRating, rating);
                return ratingStatus;
            }
        }

        void InsertNewSongRating(FbUser user, short rating)
        {
            RatingDetails songRating = new RatingDetails(user, rating);
            AddPlaylistSongRating(songRating);
        }

        void UpdatePlaylistSongRating(RatingDetails songRating, short rating)
        {
            if (rating != songRating.Rating)
            {
                base.UpdatedAggregatedRating(songRating.Rating, rating);
                songRating.LastUpdated = DateTime.Now;
                songRating.Rating = rating;
            }
        }

        protected virtual void AddPlaylistSongRating(RatingDetails songRating)
        {
            base.AddAggregatedRating(songRating.Rating);
            songRating.LastUpdated = DateTime.Now;
            songRating.PlaylistSongRating = this;
            Details.Add(songRating);
        }

        public override bool IsOwner(object id)
        {
            long _id;
            if (long.TryParse(id.ToString(), out _id))
            {
                return this.Creator.Id == _id;
            }
            return false;
        }
    }
}