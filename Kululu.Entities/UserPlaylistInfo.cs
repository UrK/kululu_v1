using System;
using System.Collections.Generic;
using Dror.Common.Data.Contracts;

namespace Kululu.Entities
{
    public class UserPlaylistInfo : IEntity
    {
        /// <summary>
        /// User with relevant points
        /// </summary>
        public virtual FbUser User { get; set; }

        /// <summary>
        /// points of the relevant user in this playlist
        /// </summary>
        public virtual Playlist Playlist { get; set; }

        /// <summary>
        /// Please view the following article for why we need to implment Equal and GetHashCode 
        /// When working with a class that has composite keys
        /// http://devlicio.us/blogs/anne_epstein/archive/2009/11/20/nhibernate-and-composite-keys.aspx
        /// </summary>

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var t = obj as UserPlaylistInfo;
            if (t == null)
                return false;
            if (User == t.User && Playlist == t.Playlist)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return (User.Id + "|" + Playlist.Id).GetHashCode();
        }  

        public virtual int NumOfSongsUsed { get; set; }

        public virtual int NumOfVotes { get; set; }

        /// <summary>
        /// points of the user in specific playlist
        /// </summary>
        public virtual int Points { get; set; }

        /// <summary>
        /// when was the user given points for entrance to the site (daily
        /// visit)
        /// </summary>
        public virtual DateTime LastEntrance { get; set; }

        /// <summary>
        /// operations being rated
        /// </summary>
        public enum Operation
        {
            DoNothing, 

            /// <summary>
            /// user adding a song to playlist
            /// </summary>
            AddSong,

            /// <summary>
            /// delete song from playlist
            /// </summary>
            DeleteSong,

            /// <summary>
            /// user rating a song in the playlist
            /// </summary>
            RateSong,

            /// <summary>
            /// User removed his rating
            /// </summary>
            RemoveSongRating,

            /// <summary>
            /// update song data
            /// </summary>
            UpdateSong,

            /// <summary>
            /// sign up for the system
            /// </summary>
            Signup,

            /// <summary>
            /// first visit of the day
            /// </summary>
            DayEntrance
        }

        private readonly static Dictionary<Operation, int> MOperationsDictionary =
            new Dictionary<Operation, int>
                {
                    {Operation.DoNothing, 0},
                    {Operation.AddSong, 10},
                    {Operation.DeleteSong, -10},
                    {Operation.RemoveSongRating, -5},
                    {Operation.RateSong, 5},
                    {Operation.Signup, 0},
                    {Operation.DayEntrance, 2}
                };


        /// <summary>
        /// Note: User must have the status of approved for him to gain points
        /// </summary>
        /// <param name="op"></param>
        public virtual void UpdateUserScore(
            Operation op)
        {
            //user must be approved in app in order to vote
            if (User.Status != Common.UserStatus.Joined
                || Playlist.IsOwner(User)) //owners of playlists don't receive points for their activity in the playlist
            {
                return;
            }

            
            Points = UpdatePoints(Points, op);

            switch (op)
            {
                case Operation.RateSong:
                    NumOfVotes++;
                    break;
                case Operation.RemoveSongRating:
                    NumOfVotes--;
                    break;
                case Operation.AddSong:
                    NumOfSongsUsed++;
                    break;
                case Operation.DeleteSong:
                    NumOfSongsUsed--;
                    break;
                case Operation.DayEntrance:
                     var ts = DateTime.Now.Subtract(LastEntrance);
                     if (ts.TotalHours > 24)
                     {
                         Points += MOperationsDictionary[Operation.DayEntrance];
                     }
                    break;
                default:
                    break;
            }
        }
        
        /// <returns>
        /// new user score
        /// </returns>
        private static int UpdatePoints(int oldScore, Operation op)
        {
            var newScore =  oldScore + MOperationsDictionary[op];
            if (newScore >= 0) //we don't save the user's points if the get to a negative value
            {
                return newScore;
            }
            else {
                return oldScore;
            }

        }

        #region Deprecated
        /// <summary>
        /// update user score in the database according to the operation
        /// performed
        /// </summary>
        ///
        /// <param name="rep">
        /// database repository to operate on
        /// </param>
        ///
        /// <param name="user">
        /// user performing the operation
        /// </param>
        ///
        /// <param name="op">
        /// operation executed by the user
        /// </param>
        ///
        /// <param name="lb">
        /// business on which this operation is performed
        /// </param>
        ///
        /// <returns>
        /// updated user points
        /// </returns>
        //public static long UpdateUserScore(
        //    IRepository rep,
        //    FbUser user,
        //    Playlist playlist,
        //    Operation op)
        //{
        //    var score = rep
        //        .Query<UserPlaylistInfo>()
        //        .Where(s => s.Playlist == playlist)
        //        .Where(s => s.User == user)
        //        .SingleOrDefault();
        //    if (score == null)
        //    {
        //        score = new UserPlaylistInfo
        //        {
        //            Playlist= playlist,
        //            User = user,
        //            Points = 0
        //        };
        //    }

        //    score.Points = UpdatePoints(score.Points, op);

        //    if (op == Operation.DayEntrance)
        //    {
        //        var ts = DateTime.Now.Subtract(score.LastEntrancePoints);
        //        if (ts.TotalHours > 24)
        //        {
        //            score.Points += MOperationsDictionary[Operation.DayEntrance];
        //        }
        //    }

        //    rep.SaveOrUpdate(score);
        //    return score.Points;
        //}

        /// <summary>
        /// calculate new user score from previous user points and operation
        /// executed
        /// </summary>
        ///
        /// <param name="oldScore">
        /// old score of the user
        /// </param>
        ///
        /// <param name="op">
        /// operation that should update the score
        /// </param>
        /// 
        /// <summary>
        /// update user points only if the last time, the user was given points
        /// for visit was 24 hours ago
        /// </summary>
        ///
        /// <param name="repo">
        /// repository being updated
        /// </param>
        ///
        /// <param name="ptsInfo">
        /// user score to update
        /// </param>
        ///
        /// <param name="lb">
        /// local business visited by the user
        /// </param>
        //private static void UpdateVisitPoints(IRepository repo, UserPlaylistInfo ptsInfo)
        //{
        //    var ts = DateTime.Now.Subtract(ptsInfo.LastEntrancePoints);
        //    if (ts.TotalHours <= 24)
        //    {
        //        return;
        //    }
        //    ptsInfo.Points += MOperationsDictionary[Operation.DayEntrance];
        //    repo.SaveOrUpdate(ptsInfo);
        //}
        #endregion

        #region IEntity Members
        public virtual bool IsOwner(object Id)
        {
            return User.Id == (long)Id;
        }

        public virtual event RepositoryTransactionCompletin OnSaving;

        public virtual void RiseSaving()
        {
            if (OnSaving!=null)
            {
                OnSaving();
            }
        }
        #endregion
    }
}
