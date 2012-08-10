using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Kululu.Web.Attributes;
using Kululu.Entities;
using AutoMapper;
using Kululu.Entities.Common;
using Dror.Common.Data.Contracts;
using Facebook;
using Kululu.Web.Models.Common;
using Kululu.Web.Models;
using Dror.Common.Utils.Contracts;
using Kululu.FBStreamHarvest;

namespace Kululu.Web.Controllers
{
    public partial class PlaylistController : BaseController
    {
        public const int NUM_OF_QUERY_SONGS = 10;

        /// <summary>
        /// User agent used by facebook when scanning pages for content
        /// </summary>
        private const string FACEBOOK_USER_AGENT = "facebookexternalhit";

        #region Auxiliary Classes

        private struct RatingInfo
        {
            public short RatingValue { get; set; }
            public DateTime Date { get; set; }
            public MemberDTO Member { get; set; }
        }

        #endregion

        public PlaylistController(IRepository repository, FacebookClient fbApp,
            ILogger logger)
            : base(repository, fbApp, logger)
        {
            
        }

        /// <summary>
        /// for Facebook crawler, the method renders metadata page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ActionResult GetVideoInfo(string id)
        {
            /* for Facebook own information retrieval render meta-data page */
            return View(MVC.Playlist.Views.VideoInfo, (object)id);
        }

        /// <summary>
        /// This method is kept here only for sake of a single post on Oz
        /// Barzilay's wall:
        /// https://www.facebook.com/NotJustMusic/posts/10150401634713632
        /// It is used nowhere else (not that we know of).
        /// The functionality was moved into GetVideoInfo
        /// </summary>
        public virtual ActionResult GetSongInfo(long id)
        {
            var playlistSong = Repository.Get<PlaylistSongRating>(id);

            /* for Facebook own information retrieval render meta-data page */
            if (HttpContext.Request.UserAgent == FACEBOOK_USER_AGENT)
            {
                var playlistSongDTO =
                    SongWithUserRatingDtoFactory.Map(playlistSong.Song, id);
                return View(MVC.Playlist.Views.SongInfo, playlistSongDTO);
            }

            /* this is user clicking the link in the post, redirect to the
             * application */
            return Redirect(string.Format("http://www.youtube.com/watch?v={0}", 
                playlistSong.Song.VideoID));
        }

        public virtual PartialViewResult Wall()
        {
            return PartialView(MVC.Playlist.Views._Wall);
        }

        public virtual PartialViewResult Dialog(string dialogName)
        {
            return PartialView(MVC.Playlist.Views._Dialogs, dialogName);
        }

        /// <summary>
        /// return partial view of specific playlist
        /// </summary>
        ///
        /// <param name="id">
        /// ID of the playlist
        /// </param>
        ///
        /// <returns>
        /// partial view with playlist and local business information
        /// </returns>
        [EnableJson]
        public virtual PartialViewResult Chart(long id)
        {
            var pls = Repository.GetUnique<Playlist>(p => p.Id == id);
            return PartialView(MVC.Playlist.Views._ChartView,
                PlaylistWithUserDTOFactory.Map(pls, BuildUserDTO(pls)));
        }

        public virtual PartialViewResult Player()
        {
            return PartialView(MVC.Playlist.Views._Player);
        }

        public virtual JsonResult GetSong(long playlistSongRatingId,
            long playlistId)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            if (playlist  == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }

            var playlistSongRating = playlist
                .Ratings
                .Where(r => r.Id == playlistSongRatingId);
            var ratingsRslt = GetSongsAndRating(playlistSongRating);
            return BuildSuccessResult(0, ratingsRslt.ToArray());
        }

        [RequireRequestValueAttribute(new[]
            {
                "localBussinesId",
                "playlistId",
                "startCount",
                "increment",
                "sort",
                "descending"
            })]
        public virtual JsonResult GetSongs(
            long localBussinesId,
            long? playlistId,
            short startCount,
            short increment,
            SortOptions sort,
            bool descending,
            long referringPlaylistSongRatingId)
        {
            if (playlistId.HasValue)
            {
                return GetSongsOfPlaylist(
                    playlistId.Value,
                    startCount,
                    increment,
                    sort,
                    descending,
                    referringPlaylistSongRatingId);
            }
            return GetAllSongsOfLocalBusiness(
                localBussinesId,
                startCount,
                increment);
        }

        private JsonResult GetAllSongsOfLocalBusiness(long localBussinesId,
            short startCount, short increment)
        {
            var ratingsList = Repository
                .GetOrderedGrandChildren<PlaylistSongRating>(
                    typeof(PlaylistSongRating),
                    "Playlist",
                    "LocalBusiness",
                    localBussinesId,
                    "FacebookAddedDate",
                    startCount,
                    increment + 1);

            var areMore = (ratingsList.Count() > increment);
            if (areMore)
            {
                ratingsList =
                    ratingsList.Take(ratingsList.Count() - 1).ToList();
            }

            var ratingsRslt = GetSongsAndRating(ratingsList);

            return BuildSuccessResult(0, new
                {
                    status = 0,
                    startCount,
                    increment,
                    areMore = areMore,
                    songs = ratingsRslt
                });
        }

        // TODO: XML-doc
        private JsonResult GetSongsOfPlaylist(
            long playlistId,
            short startCount,
            short increment,
            SortOptions sort,
            bool descending,
            long referringPlaylistSongRatingId =0)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            if (playlist == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }

            var playlistDto = PlaylistDTOFactory.Map(playlist);

            /* the query retrieves one more record than needed. This is used to
             * see if there are more results than requested (paging) */
            var ratings = playlist.GetRatings(
                startCount,
                (short)(increment + 1),
                sort,
                descending,
                CurrentUser);

            var ratingsList = ratings.ToList();
            if(referringPlaylistSongRatingId!=0)
            {
                SetReferringPlaylistSong(referringPlaylistSongRatingId, playlist, ref ratingsList);
            }

            /* limit number of returned results to the number specified by the
             * client and not one more */
            var areMore = (ratingsList.Count() > increment);
            if (areMore)
            {
                ratingsList =
                    ratingsList.Take(ratingsList.Count() - 1).ToList();
            }


            var ratingsRslt = GetSongsAndRating(ratingsList);

            var renderedData = new
                {
                    startCount,
                    increment,
                    areMore = areMore,
                    songs = ratingsRslt,
                    playlist = playlistDto
                };

            return BuildSuccessResult(0, renderedData);
        }

        private static void SetReferringPlaylistSong(
            long referringPlaylistSongRatingId,
            Playlist playlist,
            ref List<PlaylistSongRating> ratingsList)
        {
            var referalPlaylistSong = ratingsList.FirstOrDefault(
                rating => rating.Id == referringPlaylistSongRatingId);

            if (referalPlaylistSong == null)
            {
                //finding element and moving it to the first position
                referalPlaylistSong = playlist.Ratings.FirstOrDefault(
                    rating => rating.Id == referringPlaylistSongRatingId);

                //in case playlistRatin was deleted
                if (referalPlaylistSong != null)
                {
                    ratingsList.Insert(0, referalPlaylistSong);
                }
            }
            else
            {
                ratingsList.Remove(referalPlaylistSong);
                ratingsList.Insert(0, referalPlaylistSong);
            }
        }

        /// <summary>
        /// retrieve songs from a specific playlist, paging and sorting are
        /// supported.
        /// </summary>
        ///
        /// <param name="playlistSongRating">
        /// ratings of all the songs of current playlist
        /// </param>
        ///
        ///<returns>
        /// JSON encoded array of songs or null on failure
        /// </returns>
        private IList<SongWithUserRatingDTO> GetSongsAndRating(
            IEnumerable<PlaylistSongRating> playlistSongRating)
        {
            if (playlistSongRating == null)
            {
                throw new ArgumentNullException("playlistSongRating");
            }

            var rats = playlistSongRating.ToList();

            var songRatingss = rats.Select(rating => rating.Id).ToArray();

            Dictionary<long, short> songsRating = null;
            if (CurrentUser != null)
            {
                songsRating = GetSongsRatingValues(songRatingss);
            }

            var songWithRatingDtOs = new List<SongWithUserRatingDTO>();
            for (var index = 0; index < rats.Count(); index++)
            {
                var plrating = rats.ElementAt(index);
                var songWithRatingDto =
                    SongWithUserRatingDtoFactory.Map(plrating);

                //if current song was added by admin, override value with admin's
                if(songWithRatingDto.IsAddedByAdmin)
                {
                    songWithRatingDto.Member.FBID =
                        plrating.Playlist.LocalBusiness.FanPageId;
                    songWithRatingDto.Member.name =
                        plrating.Playlist.LocalBusiness.Name;
                } 

                songWithRatingDto.Rating =
                    (songsRating != null && songsRating.ContainsKey(plrating.Id)) ?
                     songsRating[plrating.Id] : (short)0;

                // if user is admin, set his vote
                if (CurrentUser!= null &&
                    CurrentUserFBInfo.IsAdmin && 
                    plrating.AdminRating != 0)
                {
                    songWithRatingDto.Rating = plrating.AdminRating;
                }

                songWithRatingDto.CreatedByCurrentUser =
                    CurrentUser != null &&
                    CurrentUser.Id == songWithRatingDto.Member.FBID;

                songWithRatingDtOs.Add(songWithRatingDto);
            }

            Mapper.CreateMap<PlaylistSongRating, SongWithUserRatingDTO>();
            return songWithRatingDtOs;
        }

        [RequireRequestValueAttribute(new[]
            {
                "playlistId", "playlistSongRatingId"
            })]
        public virtual JsonResult GetAllRatingsOfSong(
            long playlistId, long playlistSongRatingId)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            
            if (playlist == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }

            var playlistSongRating = playlist.Ratings.FirstOrDefault(
                rating => rating.Id == playlistSongRatingId);
            if (playlistSongRating == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidRatingsData);
            }

            var ratingValuesEnumerable =
                playlist.GetRatingType(playlistSongRating.Id);
            if (ratingValuesEnumerable == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidRatingsData);
            }

            var ratingValues = ratingValuesEnumerable.ToList();

            var positiveRatings = ratingValues
                .Where(r => r.Rating == (short) RatingType.Heart)
                .LongCount();


            var negativeRatings = ratingValues
                .Where(r => r.Rating == (short)RatingType.BrokenHeart)
                .LongCount();

            var neutralRatings = ratingValues
                .Where(r => r.Rating == (short)RatingType.HalfHeart)
                .LongCount();

            var extResp = ratingValues
                .Where(r => r.Rating != (short)RatingType.HalfHeart)
                .Select(r => new RatingInfo
                    {
                        RatingValue = r.Rating,
                        Date = r.LastUpdated,
                        Member = MemberDTOFactory.Map<MemberDTO>(r.VotingUser, r.PlaylistSongRating) 
                    })
                .OrderByDescending(r => r.Date).ToList();

            if(playlistSongRating.AdminRating!=0)
            {
                extResp.Insert(0, new RatingInfo
                {
                    RatingValue = playlistSongRating.AdminRating,
                    Date = 
                        playlistSongRating.FacebookAddedDate != default(DateTime)
                        ? playlistSongRating.FacebookAddedDate
                        : playlistSongRating.CreationTime,
                    Member = new MemberDTO
                        {
                            FBID = playlist.LocalBusiness.FanPageId,
                            name = playlist.LocalBusiness.Name
                        }
                        //TODO: Maybe we need to create another mapper method in DTO 
                });
            }

            switch (playlistSongRating.AdminRating)
            {
                case -1:
                    negativeRatings++;
                    break;
                case 0:
                    neutralRatings++;
                    break;
                case 1:
                    positiveRatings++;
                    break;
            }

            return BuildSuccessResult(0, new
                {
                    TotalRating =
                        negativeRatings + positiveRatings + neutralRatings,
                    NegativeRating = negativeRatings,
                    PositiveRating = positiveRatings,
                    NeutralRating = neutralRatings,
                    NumberOfVotes = ratingValues.Count(),
                    Rating = extResp.ToArray()
                });
        }

        private Dictionary<long, short> GetSongsRatingValues(
            IEnumerable<long> playlistRatingIds)
        {
            var userVotesQ = Repository.Query<RatingDetails>()
                .Where(detail => playlistRatingIds.Contains(detail.PlaylistSongRating.Id)
                    && detail.VotingUser.Id == CurrentUser.Id)
                .Select(detail => new
                    {
                        Id = detail.PlaylistSongRating.Id,
                        Rating = detail.Rating
                    });
            var userVotes = userVotesQ.ToDictionary(t => t.Id, t => t.Rating);

            return userVotes;
        }

        /// <summary>
        /// find songs in specified playlist by it's name
        /// </summary>
        ///
        /// <param name="playlistId">
        /// local ID of the playlist to look in
        /// </param>
        ///
        /// <param name="queryStr">
        /// full or partial name of the song or artist to look for or full
        /// YouTube Video ID
        /// </param>
        ///
        /// <returns>
        /// </returns>
        public virtual JsonResult QuerySongs(long playlistId, string queryStr)
        {
            /* no need to use "ToLower":
             * http://social.msdn.microsoft.com/forums/en-US/vblanguage/thread/d74851ed-2e48-4dda-a50d-9eeb149c07df/ */
            // @Uri, interesting, though it did state that it's database dependent

            var playlist = Repository.Get<Playlist>(playlistId);
            if (playlist == null)
            {
                return null;
            }
            
            queryStr = queryStr.Trim();

            var queryParams = new  Dictionary<string,string>
            {
                { "Name" , queryStr },
                { "ArtistName" , queryStr },
                { "VideoID" , queryStr }
            };

            var songsInPlaylist = Repository
                .QueryChild<PlaylistSongRating>(
                    typeof(PlaylistSongRating),
                    typeof(Playlist).Name,
                    typeof(Song).Name, playlistId,
                    queryParams,
                    NUM_OF_QUERY_SONGS);
            
            //songs wich aren't in playlist are retrieved through the song
            // itself (they aren't in playlist, therefore we cannot
            //retrieve them through their PlaylistSongRating)
            var songsNotInPlaylistDTO = new List<SongWithUserRatingDTO>();

            var songsInplaylistDTO = new List<SongWithUserRatingDTO>();
            songsInplaylistDTO.AddRange(GetSongsAndRating(songsInPlaylist));

            return BuildResult(0, string.Empty, new
                {
                    query = queryStr,
                    playlist = playlistId,
                    songsInplaylist = songsInplaylistDTO,
                    songsNotInplaylist = songsNotInPlaylistDTO
                });
        }

        public virtual JsonResult GetNumOfVotes(long playlistId)
        {
            var numOfVotes = Repository.CountGrandChildren(
                typeof(RatingDetails),
                typeof(PlaylistSongRating).Name,
                typeof(Playlist).Name,
                playlistId,
                "VotingUser",
                CurrentUser);

            return BuildResult(0, string.Empty, new
            {
                numOfVotes = numOfVotes
            });
        }

        /// <summary>
        /// add the song to the local database or update it.
        /// </summary>
        ///
        /// <param name="playlistId">
        /// ID of the playlist to add the song into
        /// </param>
        ///
        /// <param name="songName">
        /// name of the new song
        /// </param>
        ///
        /// <param name="videoId">
        /// ID of YouTube video
        /// </param>
        ///
        /// <param name="songImage">
        /// URL of the song thumbnail
        /// </param>
        ///
        /// <param name="songArtist">
        /// artist of this song
        /// </param>
        ///
        /// <param name="newRatingValue">
        /// new rating value of the added song
        /// </param>
        ///
        /// <param name="songDuration">
        /// duration of the song in seconds
        /// </param>
        ///
        ///<returns>
        /// JSON encode object with two elements: success- -1, 0, or 1
        /// according to DataBaseUpdateResponse and songId- local unique song
        /// ID.
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual JsonResult AddSong(
            long playlistId,
            string songName,
            string videoId,
            short newRatingValue,
            string songImage,
            string songArtist,
            double? songDuration = 0)
        {
            /* song being added */

            var dbResponse = DataBaseUpdateResponse.MissingValidValues;
            var playlist = Repository.Get<Playlist>(playlistId);

            var userRole = GetCurrentUserPrivileges(playlist);

            var allowedUsers =
                MatchUserPrivilege(userRole, UserPrivileges.User) ||
                MatchUserPrivilege(userRole, UserPrivileges.Owner);

            if (playlist == null)
            {
                return BuildFailureResult(
                    (short) dbResponse,
                    App_GlobalResources.Errors.invalidPlaylist);
            }

            /* rating value should be either 1 or -1 */
            if (newRatingValue != 1 && newRatingValue != -1)
            {
                return BuildFailureResult(
                    (short) DataBaseUpdateResponse.MissingValidValues,
                    App_GlobalResources.Errors.invalidRatingValue);
            }

            if (!allowedUsers)
            {
                return BuildFailureResult(
                    (short) dbResponse,
                    App_GlobalResources.Errors.invalidUser);
            }

            if (!IsVotingAllowed(playlist))
            {
                return BuildFailureResult(
                    (short) dbResponse,
                    App_GlobalResources.Errors.accessDenied);
            }

            /* checking to see if user has exceed the number of songs
             * allowed */
            var numOfSongsLeft = playlist.GetNumOfSongsLeft(CurrentUser);

            /* playlist owner can add as many songs as he wantes */
            if (numOfSongsLeft <= 0 &&
                !MatchUserPrivilege(playlist, UserPrivileges.Owner))
            {
                return BuildFailureResult(
                    (short) DataBaseUpdateResponse.LimitReached,
                    App_GlobalResources.Errors.playlistSongLimitReached);
            }

            var song = Repository.Query<Song>().FirstOrDefault(
                innerSong => innerSong.VideoID == videoId);
            if (song == null)
            {
                song = new Song();
                dbResponse = DataBaseUpdateResponse.Added;
            }
            else
            {
                dbResponse = DataBaseUpdateResponse.Updated;
            }

            song.VideoID = videoId;
            song.Name = songName;
            song.Duration = songDuration;
            song.ArtistName = songArtist;
            song.ImageUrl = songImage;
            Repository.SaveOrUpdate(song);

            /* automaticaly rating song with specified value */
            PlaylistSongRating rating;
            lock (SessionLock)
            {
                rating = playlist.AddSong(song, 
                    CurrentUser,
                    CurrentUserFBInfo.IsAdmin);

                Repository.SaveOrUpdate(rating);
                Repository.SaveOrUpdate(playlist);
            }
            SessionLock = null;

            var userDTO = BuildUserDTO(playlist, userRole);

            var songMapping = SongWithUserRatingDtoFactory.Map(rating,
                CurrentUserFBInfo.IsAdmin);
            songMapping.Rating = newRatingValue;

            var playlistDto = PlaylistDTOFactory.Map(playlist);

            var data = new
                {
                    song = songMapping,
                    user = userDTO,
                    playlist = playlistDto
                };
            return BuildResult((short) dbResponse, string.Empty, data);
        }

        private bool IsVotingAllowed(Playlist playlist)
        {
            if (MatchUserPrivilege(playlist, UserPrivileges.Owner))
            {
                return true;
            }


            //if there's no NextPlayDate give it a true value
            var isEndDateOk =
                !playlist.NextPlayDate.HasValue ||
                 playlist.NextPlayDate > DateTime.Now;

            return isEndDateOk ; //if both dates are alright
        }

        /// <summary>
        /// add one more song rating, vote for song
        /// </summary>
        ///
        /// <param name="playlistId">
        /// ID of the playlist to add this vote to
        /// </param>
        ///
        /// <param name="playlistSongRatingId">
        /// local unique ID of the rating of song being voted at
        /// </param>
        ///
        /// <param name="rating">
        /// new rating: value between -1 and 1, should be -1 for vote down,
        /// 1 for vote up, and 0 for neutral vote
        /// </param>
        ///
        /// <returns>
        /// was the vote added successfully? JSON encoded true or false
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [RequireRequestValueAttribute(new[]
            {
                "playlistId", "playlistSongRatingId", "rating"
            })]
        public virtual JsonResult RateSong(long playlistId,
            long playlistSongRatingId, short rating)
        {
            /* work with the relevant playlist */
            var playlist = Repository.Get<Playlist>(playlistId);

            /* find rating information for this song, if the song does not
                * exist, bail out */
            var ratingInfo = playlist.Ratings
                .FirstOrDefault(r => r.Id == playlistSongRatingId);
            if (ratingInfo == null)
            {
                return GetRatingResult(playlist, null, false,
                    App_GlobalResources.Errors.noRatingsForSong);
            }

            if (CurrentUser == null)
            {
                return GetRatingResult(playlist, null, false,
                    App_GlobalResources.Errors.invalidUser);
            }

            var allowedUsers =
                MatchUserPrivilege(playlist, UserPrivileges.User) ||
                MatchUserPrivilege(playlist, UserPrivileges.Owner);
            if (!allowedUsers)
            {
                return GetRatingResult(playlist, null, false,
                    App_GlobalResources.Errors.accessDenied);
            }

            if (!IsVotingAllowed(playlist))
            {
                return GetRatingResult(playlist, null, false,
                    App_GlobalResources.Errors.accessDenied);
            }

            PlaylistSongRating playlistRating;
            lock (SessionLock)
            {
                playlistRating = playlist.RateSong(ratingInfo, CurrentUser,
                    rating, CurrentUserFBInfo.IsAdmin);

                /* rating values weren't in the correct range */
                if (playlistRating == null)
                {
                    return GetRatingResult(playlist, null, false,
                        App_GlobalResources.Errors.invalidRatingValue);
                }

                Repository.SaveOrUpdate(playlistRating);
                Repository.SaveOrUpdate(playlist);
            }
            SessionLock = null;

            
            var plrating = playlistRating;

            var songWithRatingDto = SongWithUserRatingDtoFactory.Map(plrating);
            songWithRatingDto.Rating = rating;

            return GetRatingResult(playlist, songWithRatingDto, true,
                String.Empty);
        }

        /// <summary>
        /// build JSON formatted response with result of rating: success of
        /// operation and updated positive and negative ratings
        /// </summary>
        ///
        /// <param name="playlist">
        /// current playlist
        /// </param>
        ///
        /// <param name="ratingInfo">
        /// rating information to be translated
        /// </param>
        ///
        /// <param name="isRatingSuccesful">
        /// success of rating operation
        /// </param>
        ///
        /// <param name="errorMsg">
        /// error message to return as a part of the response message
        /// </param>
        ///
        ///<returns>
        /// JSON formatted object with 3 fields: Success, NegativeRating, and
        /// PositiveRating
        /// </returns>
        private JsonResult GetRatingResult(Playlist playlist,
            SongWithUserRatingDTO ratingInfo,
            bool isRatingSuccesful,
            string errorMsg)
        {
            var pls = PlaylistDTOFactory.Map(playlist);
            var rdUser = BuildUserDTO(playlist);
            var renderedData = new
                {
                    Success = isRatingSuccesful,
                    SongInfo = ratingInfo,
                    error = errorMsg,
                    playlist = pls,
                    CurrentUser = rdUser
                };

            return BuildResult(
                (short) (isRatingSuccesful ? 0 : -1),
                errorMsg,
                renderedData);
        }

        /// <summary>
        /// POST request: detach specified song from the playlist
        /// </summary>
        [HttpPost]
        public virtual JsonResult DeleteSong(long playlistId,
            long playlistSongRatingId)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            return DetachSong(playlist, playlistSongRatingId);
        }
        
        /// <summary>
        /// detach the song from the playlist
        /// </summary>
        ///
        /// <param name="playlist">
        /// playlist object to detach the song from
        /// </param>
        ///
        /// <param name="playlistSongRatingId">
        /// ID of the song rating to delete
        /// </param>
        ///
        /// <returns>
        /// result of operation with the following fields: data, ErrorMessage,
        /// and Success
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual JsonResult DetachSong(Playlist playlist,
            long playlistSongRatingId)
        {
            if (playlist == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }

            var playlistSongRating = playlist.Ratings
                .FirstOrDefault(ratings => ratings.Id == playlistSongRatingId);
            if (playlistSongRating == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.noRatingsForSong);
            }

            /* detaching song also removes all the rating of the songs in the
             * playlist */
            if (!MatchUserPrivilege(playlist, UserPrivileges.Owner) &&
                !MatchUserPrivilege(playlistSongRating, UserPrivileges.Owner))
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }
            
            var rating = playlist.DetachSong(playlistSongRating, CurrentUser);
            Repository.Delete(rating);
            Repository.Save(CurrentUser);

            var userDTO = BuildUserDTO(playlist);

            var playlistDto = PlaylistDTOFactory.Map(playlist);


            var renderedData = new
                {
                    user = userDTO,
                    playlist = playlistDto,
                    FBPostId = playlistSongRating.FBPostId
                };

            return BuildSuccessResult(0, renderedData);
        }

        /// <summary>
        /// get details of a specific playlist
        /// </summary>
        ///
        /// <param name="playlistId">
        /// JSON formatted playlist information
        /// </param>
        ///
        /// <returns>
        /// JSON rendered details of the playlist
        /// </returns>
        public virtual JsonResult GetDetails(long playlistId)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            if (playlist == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }

            var details = PlaylistDTOFactory.Map(playlist);
            if (playlist.Ratings.Any())
            {
                details.MaxVotes = (int)playlist.Ratings.Max(
                    r => (r.SummedNegativeRating + r.SummedPositiveRating));
            }
            else
            {
                details.MaxVotes = 0;
            }
            return BuildSuccessResult(0, details);
        }

        /// <summary>
        /// update song data in the database
        /// </summary>
        ///
        /// <param name="playlistId">
        /// ID of the playlist that this change appears in
        /// </param>
        ///
        /// <param name="playlistSongRatingId">
        /// ID of the song rating being edited
        /// </param>
        ///
        /// <param name="songName">
        /// new name of the song
        /// </param>
        ///
        /// <param name="artistName">
        /// new name of the artist
        /// </param>
        ///
        /// <param name="imageUrl">
        /// new URL of the image of this song (optional)
        /// </param>
        ///
        /// <returns>
        /// result of operation with "success" and "error" fields. Success
        /// will be zero in case of successful operation
        /// </returns>
        public virtual JsonResult UpdateSong(
            long playlistId,
            long playlistSongRatingId,
            string songName,
            string artistName,
            string imageUrl = null)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            
            if (playlist == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }
            
            var ratingInfo = playlist.Ratings.FirstOrDefault(
                rating => rating.Id == playlistSongRatingId);
            if (ratingInfo == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidPlaylist);
            }
            
            if (!MatchUserPrivilege(ratingInfo, UserPrivileges.Owner) &&
                !MatchUserPrivilege(playlist, UserPrivileges.Owner))
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }

            ratingInfo.Song.Name = songName;
            ratingInfo.Song.ArtistName = artistName;
            if (imageUrl != null)
            {
                ratingInfo.Song.ImageUrl = imageUrl;
            }

            Repository.Update(ratingInfo.Song);
            return BuildSuccessResult(0, null);
        }

        public virtual JsonResult GetActivityStream(long playlistId,
            ActivityType type,
            long? userId,
            DateTime? startStreamDate,
            int numOfActivities = 50)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            var activity = playlist.GetActivityStream();

            if (startStreamDate != null)
            {
                activity = activity.Where(act => act.Type == type);
            }

            if (userId != null)
            {
                activity = activity.Where(
                    act => (act != null) && (act.Actor.Id == userId));
            }

            if (type != ActivityType.All)
            {
                activity = activity.Where(act => act.Time <= startStreamDate);
            }

            activity = activity.Take(numOfActivities);


            Mapper.CreateMap<ActivityStream, ActivityStreamDTO>();

            var activityDTO = new List<ActivityStreamDTO>();
            activityDTO = Mapper.Map(activity, activityDTO);

            return BuildSuccessResult(0, activityDTO);
        }

        /// <summary>
        /// show playlist creation view
        /// </summary>
        ///
        /// <param name="fanPageId">
        /// ID of the fan page on which the playlist is created
        /// </param>
        ///
        /// <returns>
        /// view with playlist creation form
        /// </returns>
        [EnableJson]
        public virtual ActionResult Create(long fanPageId)
        {
            var lb = Repository.GetUnique<LocalBusiness>(
                l => l.FanPageId == fanPageId);

            /* not authenticating user \ demanding him to be owner of
             * localbusiness since localbusiness may not yet exit, and user
             * might haven't logged in */

            var playlistDTO = new PlaylistDTO
                    {
                        LocalBusinessId = (lb != null) ? lb.Id : -1,
                        FanPageId = fanPageId,
                        IsActive = true,
                        PublishAdminContentToWall = (lb == null) || lb.PublishAdminContentToWall,
                        PublishUserContentToWall = (lb == null) || lb.PublishUserContentToWall,
                        NumOfSongsLimit = 5 //TODO: set this magic string in some config file
                    };

            var playlistWithUserDTO = new PlaylistWithUserDTO
                {
                    Playlist = playlistDTO,
                    User = BuildUserDTO(null) //no playlist created yet
                };

            return View(MVC.Settings.Views.Create, playlistWithUserDTO);
        }

        [HttpPost]
        [EnableJson]
        public virtual ActionResult CanvasCreate(PlaylistWithUserDTO pdto)
        {
            if (!ModelState.IsValid)
            {
                return BuildFailureResult(-1, "Model is not valid");
            }

            if (Repository.Query<LocalBusiness>().Any(lb => lb.FanPageId == pdto.Playlist.FanPageId))
            {
                return BuildFailureResult(-1, "Localbusiness already exists");
            }

            var playlist = PlaylistDTOFactory.ReverseMap(pdto.Playlist);
            if (pdto.Playlist.LocalBusinessId <= 0)
            {
                //first playlist of a fan page must be active
                //Remark:value doesn't get sent right from UI, as checkbox value is disabled.
                playlist.IsActive = true;
                var localBusiness = SaveLocalBusiness(pdto.Playlist.FanPageId);
                localBusiness.AddPlaylist(playlist, true);
                localBusiness.ImportPlaylist = playlist;
                
                playlist.CreationDate = DateTime.Now;
                playlist.LastModifiedDate = DateTime.Now;

                Repository.SaveOrUpdate(localBusiness);
                Repository.SaveOrUpdate(playlist);

                ISocialStreamReader fbStreamReader = new FacebookStreamReader(
                        (advancement, status) => Console.WriteLine(status),
                        (id, entire, percentage) => Console.WriteLine(string.Format("{0}/{1}", percentage, entire)),
                        0, string.Empty, Logger, FacebookApp, Repository);

                fbStreamReader.Import(localBusiness.Id, HarvestType.TopTen);
            }

            return BuildSuccessResult(0, null);
        }

        /// <summary>
        /// handler of playlist creation post
        /// </summary>
        ///
        /// <param name="pdto">
        /// playlist editor DTO: full details of the new playlist
        /// </param>
        ///
        /// <returns>
        /// View: result of playlist creation
        /// </returns>
        [HttpPost]
        [EnableJson]
        public virtual ActionResult Create(PlaylistWithUserDTO pdto)
        {
            if (CurrentUser == null)
            {
                return View(MVC.Shared.Views.Error, new ErrorMessage(
                    App_GlobalResources.Errors.userNotStored));
            }

            if (ModelState.IsValid)
            {
                var playlist = PlaylistDTOFactory.ReverseMap(pdto.Playlist);

                LocalBusiness localBusiness;
                if (pdto.Playlist.LocalBusinessId <= 0)
                {
                    /* if current user is not admin of page he cannot add a
                     * localbusiness */
                    if (!CurrentUserFBInfo.IsAdmin)
                    {
                        return View(MVC.Shared.Views.Error,
                            new ErrorMessage(
                                App_GlobalResources.Errors.accessDenied));
                    }

                    //first playlist of a fan page must be active
                    //Remark:value doesn't get sent right from UI, as checkbox value is disabled.
                    playlist.IsActive = true;
                    localBusiness = SaveLocalBusiness(pdto.Playlist.FanPageId);
                    localBusiness.AddOwner(CurrentUser);
                    localBusiness.AddPlaylist(playlist, true);
                }
                else
                {
                    localBusiness = Repository.Get<LocalBusiness>(
                        pdto.Playlist.LocalBusinessId);
                    localBusiness.AddPlaylist(playlist);
                }

                //user must own localbusiness for him to add a playlist
                if (!localBusiness.IsOwner(CurrentUser.Id))
                {
                    return View(MVC.Shared.Views.Error, new ErrorMessage(
                        App_GlobalResources.Errors.accessDenied));
                }


                /* refresh access token for this page if requred */
                try
                {
                    var accessToken = 
                        Dror.Common.Utils.Facebook.GetPageAccessToken(
                            FacebookApp,
                            playlist.LocalBusiness.FanPageId,
                            CurrentUser.Id);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        playlist.LocalBusiness.FBFanPageAccessToken =
                            accessToken;
                    }
                }
                catch (FacebookApiException e)
                {
                    var msg = String.Format(
                        "Failed to retrieve fan page {0} token for user {1}.",
                        localBusiness.FanPageId, CurrentUser.Id);
                    Logger.ErrorFormat(msg, e);
                }

                playlist.CreationDate = DateTime.Now;
                playlist.LastModifiedDate = DateTime.Now;

                Repository.SaveOrUpdate(localBusiness);
                Repository.SaveOrUpdate(playlist);

                ISocialStreamReader fbStreamReader = new FacebookStreamReader(
                        (advancement, status) => Console.WriteLine(status),
                        (id, entire, percentage) => Console.WriteLine(string.Format("{0}/{1}", percentage, entire)),
                        0, string.Empty, Logger, FacebookApp, Repository);

                fbStreamReader.Import(localBusiness.Id, HarvestType.TopTen);

                return RedirectToAction("Settings", "LocalBusiness",
                    new RouteValueDictionary(
                        new { playlist.LocalBusiness.Id }));
            }

            //TODO: Check why userDTO returns null;
            pdto.User = BuildUserDTO(null);
            return View(MVC.Settings.Views.Create, pdto);
        }

        private LocalBusiness SaveLocalBusiness(long pageId)
        {
            var fbQueryUrl = string.Format("/{0}", pageId);
            dynamic fanPageInfo = FacebookApp.Get(fbQueryUrl);

            var localBusiness = new LocalBusiness
            {
                FanPageId = pageId,
                Name = fanPageInfo.name,
                FacebookUrl = fanPageInfo.link,
                Category = fanPageInfo.category,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                PublishAdminContentToWall = true,
                PublishUserContentToWall = true
            };

            if (fanPageInfo.location != null)
            {
                localBusiness.AddressCity = fanPageInfo.location.city;
                localBusiness.AddressStreet = fanPageInfo.location.street;
            }

            Repository.Save(localBusiness);
            return localBusiness;
        }

        [EnableJson]
        public virtual ActionResult Edit(long id)
        {
            var playlist = Repository.Get<Playlist>(id);
            var rslt = IsUserAllowedToEditEntity(playlist);
            if (rslt != null) return rslt;

            if (!MatchUserPrivilege(playlist, UserPrivileges.Owner))
            {
                return View(MVC.Shared.Views.Error, new ErrorMessage(
                    App_GlobalResources.Errors.accessDenied));
            }

            var usrDTO = BuildUserDTO(playlist);
            var editInfo = PlaylistWithUserDTOFactory.Map(playlist, usrDTO);
            return View(MVC.Settings.Views.Edit, editInfo);
        }

        [HttpPost]
        public virtual ActionResult Edit(PlaylistWithUserDTO editInfo)
        {
            if (ModelState.IsValid)
            {
                var playlist = Repository.Get<Playlist>(editInfo.Playlist.Id);

                var rslt = IsUserAllowedToEditEntity(playlist);
                if (rslt != null) return rslt;

                playlist.Name = editInfo.Playlist.Name;
                playlist.Description = editInfo.Playlist.Description;
                playlist.NumOfSongsLimit = editInfo.Playlist.NumOfSongsLimit;
                playlist.NextPlayDate = editInfo.Playlist.NextPlayDate;
                playlist.IsActive = editInfo.Playlist.IsActive;
                playlist.IsSongsLimitDaily =
                    editInfo.Playlist.IsSongsLimitDaily;

                /* refresh access token for this page if requred */
                try
                {
                    var accessToken =
                        Dror.Common.Utils.Facebook.GetPageAccessToken(
                            FacebookApp,
                            playlist.LocalBusiness.FanPageId,
                            CurrentUser.Id);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        playlist.LocalBusiness.FBFanPageAccessToken =
                            accessToken;
                    }
                }
                catch (FacebookApiException e)
                {
                    var msg = String.Format(
                        "Failed to retrieve fan page {0} token for user {1}.",
                        playlist.LocalBusiness.FanPageId, CurrentUser.Id);
                    Logger.ErrorFormat(msg, e);
                }
                
                Repository.Update(playlist);

                return RedirectToAction(
                    "Settings",
                    "LocalBusiness",
                    new RouteValueDictionary(new { playlist.LocalBusiness.Id }));
            }

            //TODO: Check why userDTO returns null;
            editInfo.User = BuildUserDTO(null);
            return View(MVC.Settings.Views.Edit, editInfo);
        }

        /// <summary>
        /// override of BaseController method.
        /// This one checks if the user is co-owner of the playlist
        /// </summary>
        ///
        /// <param name="entity">
        /// entity (playlist) to be tested for co-ownership
        /// </param>
        //
        /// <returns>
        /// privileges of the the current user
        /// </returns>
        protected new UserPrivileges GetCurrentUserPrivileges(IEntity entity)
        {
            /* Check if values are set */
            if (IsCurrentUserNotSet() || entity == null)
            {
                return UserPrivileges.None;
            }

            /* current user privileges */
            var priv = base.GetCurrentUserPrivileges(entity);

            /* if the user is already owner of this entity, just return */
            if (priv == UserPrivileges.Owner)
            {
                return UserPrivileges.Owner;
            }

            /* if the object is not playlist, we don't know how to handle it
             * here */
            if (!(entity is Playlist))
            {
                return priv;
            }

            /* check if the current user is the coowner of the list */
            var pls = (Playlist) entity;
            return pls.LocalBusiness.Owners.Contains(CurrentUser) ?
                UserPrivileges.Owner : priv;
        }
    }
}
