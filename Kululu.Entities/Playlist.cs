using System;
using System.Collections.Generic;
using System.Linq;
using Dror.Common.Data.Contracts;
using Kululu.Entities.Common;
using Dror.Common.Extensions;

namespace Kululu.Entities
{
    public class Playlist : IEntity, IActivity
    {
        #region Properties
        public virtual long Id { get; set; }

        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime LastModifiedDate { get; set; }
        public virtual DateTime? LastPlayedDate { get; set; }
        public virtual DateTime? NextPlayDate { get; set; }

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Image { get; set; }
        public virtual int NumOfSongsLimit { get; set; }

        public virtual LocalBusiness LocalBusiness { get; set; }
        public virtual IList<Visit> Visits { get; set; }

        public virtual IList<PlaylistSongRating> Ratings { get; set; }
        public virtual IList<UserPlaylistInfo> UserInfo { get; set; }

        public virtual int NumberOfSongs { get; set; }
        public virtual int NumberOfVotes { get; set; }
        public virtual bool IsSongsLimitDaily { get; set; }

        /// <summary>
        /// does this playlist post news feed to the wall periodically?
        /// </summary>
        public virtual bool IsPushesToWall { get; set; }

        public virtual string UpdateTitleFormat { get; set; }
        public virtual string UpdateDescriptionFormat { get; set; }

        /// <summary>
        /// when should the next update update occur?
        /// </summary>
        public virtual DateTime NextUpdate { get; set; }

        /// <summary>
        /// Get/set value of next update time
        /// </summary>
        public virtual DateTime NextUpdateTime
        {
            get
            {
                //checking for default values
                if (NextUpdate == DateTime.MinValue)
                {
                    NextUpdate = DateTime.Now.AddHours(IncrementUpdate);
                }
                return NextUpdate;
            }
            set
            {
                NextUpdate = value;
            }
        }
        public virtual short IncrementUpdate { get; set; }

        /// <summary>
        /// is this list user modifyable?
        /// </summary>
        public virtual bool IsUserModifyable { get; set; }

        #endregion

        #region ctor
        /// <summary>
        /// default playlist consctuctor
        /// </summary>
        public Playlist()
        {
            CreationDate = DateTime.Now;
            Ratings = new List<PlaylistSongRating>();
            UserInfo = new List<UserPlaylistInfo>();

            #region default values
            IsActive = true;
            UpdateTitleFormat = "עדכון חם מהשטח, השירים המובילים שלנו הם:";
            IncrementUpdate = 24;

            #endregion
        }
        #endregion

        #region Methods

        public virtual void AddUserInfo(UserPlaylistInfo userInfo)
        {
            if (!UserInfo.Contains(userInfo))
            {
                userInfo.Playlist = this;
                UserInfo.Add(userInfo);
                //and adding the reference to the playlist on the other side as well
                userInfo.User.PlaylistsInfo.Add(userInfo);
            }
        }

        public virtual void RemoveUserInfo(UserPlaylistInfo userInfo)
        {
            if (!UserInfo.Contains(userInfo))
            {
                userInfo.Playlist = null;
                UserInfo.Add(userInfo);
                userInfo.User.PlaylistsInfo.Remove(userInfo);
            }
        }

        #region Rating

        private PlaylistSongRating GetCurrentStatistics(Song song)
        {
            lock (GetType())
            {
                var currentStatistics = Ratings
                    .Where(s => s.Song.Id == song.Id)
                    .Where(s => s.Playlist == this)
                    .Select(s => s)
                    .FirstOrDefault();
               
                return currentStatistics;
            }
        }

        public virtual bool DoesRatingExist(Song song, FbUser ratingUser)
        {
            var currentStatistics = GetCurrentStatistics(song);

            if (currentStatistics == null)
            {
                return false;
            }
            return currentStatistics.Details.Any(_details => _details.VotingUser.Id == ratingUser.Id);
        }

        /// <summary>
        /// add single rating to the database or update it with new value
        /// </summary>
        ///
        /// <param name="song">
        /// song to add the rating to
        /// </param>
        ///
        /// <param name="ratingUser">
        /// who's adding this rating?
        /// </param>
        ///
        /// <param name="rating">
        /// rating value: either 1 or -1
        /// </param>
        ///
        /// <returns>
        /// A boolean indicating if rating was in the right input range (between -1 to 1)
        /// </returns>
        public virtual PlaylistSongRating RateSong(PlaylistSongRating playlistSongRating, FbUser ratingUser, short rating,  bool isAddedByAdmin = false, bool isNewSong = false)
        {
            if (PlaylistSongRating.IsRatingValueInvalid(rating))
            {
                return null;
            }

            RatingStatus ratingType = RatingStatus.NothingNew;
            //var currentStatistics = GetCurrentStatistics(song);

            if (isAddedByAdmin) //if admin votes the song give it a special treatment
            {                   
                if (playlistSongRating.AdminRating != 0 && rating == 0)    //admin just cleared his vote
                {
                    ratingType = RatingStatus.RemovedRating;
                }

                if (playlistSongRating.AdminRating == 0 && rating != 0) //admin switched between positive and negative score
                {                                                     
                    ratingType = RatingStatus.AddedNewRating;
                }

                if (playlistSongRating.AdminRating == 0 && rating == 0) //admin added song but didn't rate it (can be achieved through harvesting)
                {
                    ratingType = RatingStatus.AddedNewEmptyRating;
                }

                playlistSongRating.SetAdminRating(playlistSongRating.AdminRating, rating, isNewSong);
                playlistSongRating.AdminRating = rating;
            }
            else 
            {
                ratingType = playlistSongRating.AddRating(ratingUser, rating);
            }

            //note: AddRating also adds points to user as well as advances the NumberOfVotes and NumberOfSongs counters
            UpdateAggregatedInfo(ratingType, ratingUser, isNewSong, isAddedByAdmin);
            return playlistSongRating;
        }

        public virtual PlaylistSongRating AddSong(Song song, FbUser user, bool addedByAdmin, short rating = 1)
        {
            var playlistSongRating = new PlaylistSongRating
            {
                Playlist = this,
                Song = song,
                Creator = user
            };
            Ratings.Add(playlistSongRating);

            var palylistSongRating = RateSong(playlistSongRating, user, rating, addedByAdmin, true);

            if (addedByAdmin)
            {
                palylistSongRating.IsAddedByAdmin = true;
            }

            return palylistSongRating;
        }

        public virtual PlaylistSongRating DetachSong(PlaylistSongRating playlistSongRating, FbUser user)
        {
            if (playlistSongRating != null)
            {
                Ratings.Remove(playlistSongRating);
                NumberOfVotes -= playlistSongRating.Details.Count(detail=>detail.Rating!=0);
                NumberOfVotes -= Math.Abs(playlistSongRating.AdminRating);
                NumberOfSongs--;

                //TODO: enhance this efficiency, move this to nhibernate layer. Currently alright for this usage, but might be heavy.
                foreach (var detail in playlistSongRating.Details.Select(_detail=>_detail.VotingUser))
                {
                    var userPlaylistInfo = detail.PlaylistsInfo.FirstOrDefault(pInfo => pInfo.Playlist == this);
                    if(userPlaylistInfo!= null)
                        userPlaylistInfo.NumOfVotes--;
                }

                //we don't reward the owner of the playlist, it's not fair
                if (playlistSongRating.IsOwner(user.Id))
                {
                    RewardUser(user, UserPlaylistInfo.Operation.DeleteSong);
                    if (playlistSongRating.Details.Any(details => 
                        details.VotingUser == user
                        && details.Rating!=0))
                    {
                        RewardUser(user, UserPlaylistInfo.Operation.RemoveSongRating);
                    }
                }
            }
            return playlistSongRating;
        }

        public virtual Song GetSongInPlaylist(long songId)
        {
            return Ratings.FirstOrDefault(rating => rating.Song.Id == songId)
                          .Song;
        }

        private PlaylistSongRating GetPlaylistSongRating(long songId)
        {
            return Ratings.FirstOrDefault(rating => rating.Song.Id == songId);
        }

        #endregion Rating

        public virtual IEnumerable<PlaylistSongRating> GetRatings(
            short skip,
            short incremenet,
            SortOptions sort,
            bool descending,
            FbUser currentUser = null)
        {
            IEnumerable<PlaylistSongRating> songsRating = Ratings;
            switch (sort)
            {
                case SortOptions.None:
                    break;
                case SortOptions.DateAdded:
                    songsRating = Ratings.OrderByWithDirection(playlistRatings => playlistRatings.FacebookAddedDate, descending);
                    break;
                case SortOptions.Popularity:
                    //most voted for songs (not heighest rated songs though)
                    songsRating = Ratings.OrderByWithDirection(playlistRatings => playlistRatings.SummedNegativeRating + playlistRatings.SummedPositiveRating, descending);
                    break;
                case SortOptions.Rating:
                    songsRating = Ratings.OrderByWithDirection(playlistRatings => playlistRatings.SummedPositiveRating, descending);
                    break;
                case SortOptions.Me :
                    return songsRating = Ratings.Where(playlistRatings =>
                                                      playlistRatings.Details.Any(details => details.VotingUser == currentUser));
                default:
                    break;
            }

            return songsRating.Skip(skip).Take(incremenet);
        }

        public virtual Song[] FilterOnlySongsInPlaylist(Song[] songs)
        {
            var songsInWedding = new List<Song>();
            for (var index = 0; index < songs.Length; index++)
            {
                var statistic = Ratings.Where(stats => stats.Song.Id == songs[index].Id)
                                                 .FirstOrDefault();

                if (statistic != null)
                {
                    var foundSong = statistic.Song;
                    songsInWedding.Add(foundSong);
                }
            }
            return songsInWedding.ToArray();
        }

        public virtual IEnumerable<RatingDetails> GetRatingType(long playlistSongRating)
        {
            var rats = Ratings
                .Where(s => s.Id == playlistSongRating)
                .FirstOrDefault();

            return (rats == null) ? null : rats.Details;
        }

        public virtual RatingType[] GetRatingType(long fbUserId, long[] songIds)
        {
            var queriedRatings =
                Ratings.Where(rating => songIds.Contains(rating.Song.Id)).ToArray();

            
            /* in case user didn't vote for all songs, we need to have the
             * array in the size of the original given array and fill in values
             * for those songs that the user has voted for */
            var ratingType = new RatingType[songIds.Length];

            for (int index = 0; index < songIds.Length; index++)
            {
                /* only fill in the voting in the index of the songs the user
                 * really did vote for */
                var foundSongStatisicts = queriedRatings
                    .Where(rating => rating.Song.Id == songIds[index])
                    .FirstOrDefault();
                if (foundSongStatisicts != null)
                {
                    ratingType[index] = foundSongStatisicts.GetCurrentUserRating(fbUserId);
                }
            }
            return ratingType;
        }

        public virtual bool HasUserRatedSong(Song song, FbUser CurrentUser)
        {
            var queriedWeddingSongsStats =
                Ratings.Where(weddingSongsStas => weddingSongsStas.Song.Id == song.Id).FirstOrDefault();

            if (queriedWeddingSongsStats == null)
            {
                return false;
            }

            var ratedSong = 
                    queriedWeddingSongsStats.Details.ToList()
                                            .Where(songRating => songRating.VotingUser.Id == CurrentUser.Id)
                                            .FirstOrDefault();
            return (ratedSong != null);
        }

        public virtual IEnumerable<ActivityStream> GetActivityStream()
        {
            var votedActivityStream = 
                from ratingInfo in Ratings.Select(rating=>rating.Details)
                    select ratingInfo.Select<RatingDetails,ActivityStream>(
                        _ratingInfo => new ActivityStream() { 
                            Time = _ratingInfo.LastUpdated, 
                            Actor = _ratingInfo.VotingUser,   
                            Verb = _ratingInfo.Verb,
                            Object = _ratingInfo.PlaylistSongRating.Song,
                            Type = ActivityType.RatedSong
                        });

            var aggregatedVotedActivityStream = votedActivityStream.Aggregate((first, second) => first.Union(second));

             var addedSongActivityStream = 
                    from ratingInfo in Ratings
                    select new ActivityStream(){
                        Time = ratingInfo.CreationTime,
                        Actor = ratingInfo.Creator,
                        Object = ratingInfo.Song,
                        Verb = ratingInfo.Song.Verb,
                        Type = ActivityType.AddedSong
                    };

             var userActivityStream =
                  from visitInfo in Visits
                  orderby visitInfo.VisitDate ascending
                  group visitInfo by visitInfo.User;

             var userJoinedActivityStream =
                     from userJoined in userActivityStream.Select(_userActivityStream => _userActivityStream.FirstOrDefault())
                     select new ActivityStream()
                       {
                           Time = userJoined.VisitDate,
                           Actor = userJoined.User,
                           Object = userJoined.Playlist,
                           Verb = userJoined.User.Verb,
                           Type = ActivityType.UserJoined
                       };

            aggregatedVotedActivityStream.Union(addedSongActivityStream);
            aggregatedVotedActivityStream.Union(userJoinedActivityStream);

            return aggregatedVotedActivityStream;
        }

        #region Crud
        //public static Playlist Get(long weddingId)
        //{
        //    var session = BaseEntity.Session;
        //    return (from wedding in session.Linq<Playlist>()
        //            where wedding.Id == weddingId
        //            select wedding).FirstOrDefault();
        //}

        //public static Playlist Get(string weddingName)
        //{
        //    var session = BaseEntity.Session;
        //    return (from wedding in session.Linq<Playlist>()
        //            where wedding.WeddingName == weddingName
        //            select wedding).FirstOrDefault();
        //}

        //public static List<Playlist> Get(int startIndex, int increment)
        //{
        //    var session = BaseEntity.Session;
        //    return (from wedding in session.Linq<Playlist>()
        //            select wedding)
        //            .Skip(startIndex)
        //            .Take(increment).ToList();
        //}

        //public virtual void Save(bool commitTransaction = false)
        //{
        //    WeddingName = GetFreeWeddingName();
        //    base.Save(commitTransaction);
        //}
        #endregion Crud

        #region Helpers

        //public virtual string GetFreeWeddingName()
        //{
        //    bool isWeddingNameFree = false;
        //    string weddingName = string.Format("{0}❤{1}", this.Bride.Name, this.Groom.Name);
        //    weddingName = weddingName.Replace(" ", "");
        //    int? increment = null;
        //    while (!isWeddingNameFree)
        //    {
        //        weddingName = string.Format("{0}{1}", weddingName, increment);
        //        isWeddingNameFree = IsWeddingNameFree(weddingName);
        //        increment = increment == null ? 0 : 0;
        //        increment++;
        //    }
        //    return weddingName;
        //}

        //bool IsWeddingNameFree(string weddingName)
        //{
        //    return Playlist.Get(weddingName) == null;
        //}
        
        #endregion Helpers

        public virtual bool IsOwner(object id)
        {
            long parsedId;
            if (long.TryParse(id.ToString(), out parsedId))
            {
                return LocalBusiness.Owners.Any(o => o.Id == parsedId);
            }
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

        /// <summary>
        /// get number of songs used by current user
        /// </summary>
        ///
        /// <param name="user">
        /// user to test
        /// </param>
        ///
        /// <returns>
        /// number of songs left for the user to vote on
        /// </returns>
        public virtual int GetNumOfSongsLeft(FbUser user)
        {
            var numOfSongsUsed = 0;

            if (IsSongsLimitDaily)
            {
                numOfSongsUsed = Ratings.Where(
                        r => r.CreationTime > DateTime.Now.AddDays(-1))
                    .Where(r => r.Creator == user)
                    .Count();
            }
            else
            {
                var userInfo = UserInfo.FirstOrDefault(ui => ui.User == user);
                if (userInfo != null)
                {
                    numOfSongsUsed = userInfo.NumOfSongsUsed;
                }
            }
            return NumOfSongsLimit  - numOfSongsUsed;
        }


        private void UpdateAggregatedInfo(RatingStatus ratingStatus, FbUser ratingUser, bool isNewSong, bool isAdmin = false)
        {
            switch (ratingStatus)
            {
                case RatingStatus.AddedNewEmptyRating:
                    if (isNewSong)
                    {
                        NumberOfSongs++;
                        RewardUser(ratingUser, UserPlaylistInfo.Operation.AddSong, isAdmin);
                    }
                    break;
                case RatingStatus.AddedNewRating:
                    if (isNewSong)
                    {
                        NumberOfSongs++;
                        NumberOfVotes++;
                        RewardUser(ratingUser, UserPlaylistInfo.Operation.AddSong, isAdmin);
                    }
                    else
                    {
                        NumberOfVotes++;
                    }
                    RewardUser(ratingUser, UserPlaylistInfo.Operation.RateSong, isAdmin);
                    break;
                case RatingStatus.AddedRating:
                    RewardUser(ratingUser, UserPlaylistInfo.Operation.RateSong, isAdmin);
                    NumberOfVotes++;
                    break;
                case RatingStatus.RemovedRating:
                    NumberOfVotes--;
                    RewardUser(ratingUser, UserPlaylistInfo.Operation.RemoveSongRating, isAdmin);
                    break;
                default:
                    break;
            }
        }

        private void RewardUser(FbUser user,UserPlaylistInfo.Operation options, bool isAdmin = false)
        {
            if (isAdmin || user == null)
            {
                return;
            }

            var userInfo = GetUserInfo(user);
            if (userInfo != null)
            {
                userInfo.UpdateUserScore(options);
            }
        }

        public virtual UserPlaylistInfo GetUserInfo(FbUser user)
        {
            var usrPlylistInfo = UserInfo.FirstOrDefault(info => info.User == user);
            if (usrPlylistInfo == null && !IsOwner(user.Id)) //admins don't need this connection to the playlist
            {
                usrPlylistInfo = new UserPlaylistInfo
                    {
                        NumOfSongsUsed = 0,
                        User = user,
                        Points = 0,
                        LastEntrance = DateTime.Now
                    };
                AddUserInfo(usrPlylistInfo);
            }
            return usrPlylistInfo;
        }
    }
    #endregion
}

