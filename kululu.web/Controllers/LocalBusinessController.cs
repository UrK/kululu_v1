using System;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using System.Web.Routing;
using Facebook;
using Kululu.Web.Attributes;
using Kululu.Entities;
using Dror.Common.Data.Contracts;
using Kululu.Web.Models;
using Kululu.Entities.Common;
using System.Web.Script.Serialization;
using Dror.Common.Utils.Contracts;
using Dror.Common.Utils;
using System.Collections.Generic;

namespace Kululu.Web.Controllers
{
    [HandleError]
    public partial class LocalBusinessController : BaseController
    {
        /// <summary>
        /// number of users returned for a regular leader board request
        /// </summary>
        private const int LEADER_BOARD_SIZE = 10;

        /// <summary>
        /// number of songs appearing in periodic post
        /// </summary>
        private const int PERIODIC_POST_SONGS = 3;

        /// <summary>
        /// URL of YouTube thumbnails
        /// </summary>
        private const string YOUTUBE_THUMBNAIL_URL =
            "https://img.youtube.com/vi/{0}/default.jpg";

        /// <summary>
        /// default image 
        /// </summary>
        private const string DEFAULT_PERIODIC_POST_IMAGE =
            "Content/img/default_periodic.png";

        public LocalBusinessController(IRepository repository, FacebookClient fbApp, ILogger logger)
            : base(repository, fbApp, logger)
        {
        }

        [EnableJson]
        public virtual ActionResult GetInfo(long id)
        {
            var localBussiness = Repository.Get<LocalBusiness>(id);
            if (localBussiness == null)
                return null;

            if (HttpContext.Request.UrlReferrer == null)
            {
                var cust = Repository
                    .Query<Customizations>()
                    .FirstOrDefault(c => c.LocalBusiness == localBussiness);
                var lbLocale = (cust != null) ? cust.Locale : null;

                var localBusinessDTO =
                    LocalBusinessDTOFactory.Map<LocalBusinessDTO>(
                        localBussiness,
                        HttpContext.Request.ApplicationPath,
                        lbLocale);
                return View(MVC.Settings.Views.LocalBusinessInfo, localBusinessDTO);
            }

            var pageUrl = GetAppPageUrl(localBussiness.FanPageId);
            Response.Redirect(pageUrl);
            return null;
        }

     
        [EnableJson]
        public virtual ActionResult Index()
        {
            const long fanPageId = 178456328876503;
            var localBusiness = Repository
                .Query<LocalBusiness>()
                .FirstOrDefault(plid => plid.FanPageId == fanPageId);
            if (localBusiness == null)
            {
                return null;
            }

            var playlist = localBusiness.DefaultPlaylist;
            var playlistsDTO = PlaylistDTOFactory.Map(playlist);
            return null;

            //return View(MVC.Mobile.Views.PlaylistMobile, playlistsDTO);
        }

        [RequireRequestValueAttribute(new[] { "startCount", "increment" })]
        public virtual JsonResult Index(short startCount, short increment)
        {
            var playlists = Repository.Query<Playlist>(startCount, increment).ToList();
            var playlistsDto = PlaylistDTOFactory.Map(playlists);
            return BuildSuccessResult(0, playlistsDto);
        }

        public virtual ActionResult SetDefaultPlaylist(long localBusinessId, long playlistId)
        {
            var localBusiness = Repository.Get<LocalBusiness>(localBusinessId);
            var rslt = IsAuthorized(localBusiness);
            if (rslt != null) return rslt;

            var playlist = Repository.Get<Playlist>(playlistId);
            playlist.IsActive = true;
            localBusiness.DefaultPlaylist = playlist;
            
            Repository.SaveOrUpdate(localBusiness);
            return RedirectToAction(
                 "Settings",
                 "LocalBusiness",
                 new RouteValueDictionary(new { id = localBusinessId }));
        }

        /// <summary>
        /// return default playlist of specified fan page
        /// </summary>
        ///
        /// <param name="fanPageId">
        /// ID of the fan page to retrieve the data from
        /// </param>
        ///
        /// <returns>
        /// View with requested playlist or error view is the playlist does not
        /// exists
        /// </returns>
        [EnableJson]
        public virtual ActionResult GetPlaylistByFanPage(long fanPageId)
        {
            var localBusiness = Repository.GetUnique<LocalBusiness>(
                lb => lb.FanPageId == fanPageId);

            if (localBusiness == null)
            {
                return View(MVC.Shared.Views.Error, new ErrorMessage(
                    App_GlobalResources.Errors.fanPageNotFound));
            }

            var playlist = localBusiness.DefaultPlaylist;
            if (playlist == null)
            {
                return View(MVC.Shared.Views.Error, new ErrorMessage(
                    App_GlobalResources.Errors.noDefaultPlaylist));
            }

            return View(MVC.Playlist.Views.Playlist,
                BuildPageInfoDto(localBusiness, LoadStrings()));
        }

        public virtual JsonResult GetPlaylists(long id)
        {
            var localBusiness = Repository.GetUnique<LocalBusiness>(
                lb => lb.Id == id);
            if (localBusiness == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.fanPageNotFound);
            }

            var playlistsDTO = PlaylistDTOFactory.Map(localBusiness.Playlists);
            return BuildSuccessResult(1, playlistsDTO);
        }

        [EnableJson]
        public virtual ActionResult GetPlaylist(long id)
        {
            var playlist = Repository.GetUnique<Playlist>(ip => ip.Id == id);
            if (playlist == null)
            {
                return View(MVC.Shared.Views.Error, new ErrorMessage(
                    App_GlobalResources.Errors.invalidPlaylist));
            }

            var playlistsDTO = PlaylistDTOFactory.Map(playlist);
            playlistsDTO.MaxVotes = (int)playlist.Ratings
                .Max(r => (r.SummedNegativeRating + r.SummedPositiveRating));

            return View(MVC.Playlist.Views.Playlist,
                new PageInfoDTO
                    {
                        Playlists = new[] { playlistsDTO }
                    });
        }

        public virtual JsonResult AddOwner(long localBusinessId, long ownerId)
        { 
            var user = Repository.Get<FbUser>(ownerId);
            if (user == null)
            {
                user = new FbUser(ownerId) {Status = UserStatus.Pending};
                Repository.Save(user);
            }

            var localBusiness = Repository.Get<LocalBusiness>(localBusinessId);
            var rslt = IsAuthorized(localBusiness);
            if(rslt != null ) return rslt ;

            localBusiness.AddOwner(user);
            Repository.SaveOrUpdate(localBusiness);

            return BuildSuccessResult(0, string.Empty);
        }

        public virtual JsonResult RemoveOwner(long localBusinessId, long ownerId)
        {
            var user = Repository.Get<FbUser>(ownerId);

            if (user == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }
            if (user == CurrentUser) //user cannot remove himself from administrating a page
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.lastAdminCannotBeRemoved);
            }

            var localBusiness = Repository.Get<LocalBusiness>(localBusinessId);
            localBusiness.RemoveOwner(user);
            Repository.SaveOrUpdate(localBusiness);
            return BuildSuccessResult(0, string.Empty);
        }

        /// <summary>
        /// parse the culture specified for local business
        /// </summary>
        ///
        /// <param name="lb">
        /// local business for saving the data
        /// </param>
        ///
        /// <param name="prop">
        /// property to process
        /// </param>
        ///
        /// <returns>
        /// true if the property was processed by this method, false otherwise
        /// </returns>
        private bool ParseCulture(LocalBusiness lb,
            KeyValuePair<string, object> prop)
        {
            if (!prop.Key.Equals("Locale"))
            {
                return false;
            }

            /* find customization for general local business (without looking
             * at playlist */
            var cust = Repository
                .Query<Customizations>()
                .Where(c => c.LocalBusiness == lb)
                .Where(c => c.Playlist == null)
                .FirstOrDefault();

            /* try to parse the locale, if failed return "true" marking
             * completion of property processing */
            var locs = Enum.GetNames(typeof (MvcApplication.Culture));
            if (!locs.Contains(prop.Value.ToString()))
            {
                Logger.WarnFormat("Invalid locale specified: {0}", prop.Value);
                return true;
            }

            if (cust == null)
            {
                cust = new Customizations { LocalBusiness = lb };
            }
            cust.Locale = prop.Value.ToString();
            Repository.SaveOrUpdate(cust);
            return true;
        }

        /// <summary>
        /// handler of social setting page "Save" button
        /// </summary>
        ///
        /// <param name="id">
        /// ID of the local business
        /// </param>
        ///
        /// <param name="properties">
        /// JSON formatted list of properties with values
        /// </param>
        ///
        /// <returns>
        /// JSON formatted saving result
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual JsonResult SetSocialSettings(long id, string properties)
        {
            var serializer = new JavaScriptSerializer();
            var props = serializer.Deserialize<Dictionary<string, object>>(
                properties);

            var localBusiness = Repository.Get<LocalBusiness>(id);
            var rslt = IsAuthorized(localBusiness);
            if (rslt != null)
            {
                return rslt;
            }

            try
            {
                foreach (var prop in props)
                {
                    // if we have connected entities we might get a property
                    // with the following syntex: Playlist_Id.
                    // we just want the playlist part
                    var propKey = prop.Key.Split('_')[0];

                    var property = localBusiness.GetType().GetProperty(propKey);

                    /* process extra properties: the ones that are not part of
                     * either local business or playlist entities */
                    var extraProps = ParseCulture(localBusiness, prop);
                    if (extraProps)
                    {
                        continue;
                    }

                    if (property == null)
                    {
                        property = localBusiness
                            .DefaultPlaylist
                            .GetType()
                            .GetProperty(propKey);
                        property.SetValue(localBusiness.DefaultPlaylist,
                            prop.Value, null);
                    }
                    else
                    {
                        // handling complex connected entities
                        if (property.PropertyType.IsEnum)
                        {
                            var val = Enum.Parse(property.PropertyType,
                                prop.Value.ToString(), true);
                            property.SetValue(localBusiness, val, null);
                        }
                        else if (property.PropertyType.Assembly ==
                            typeof(LocalBusiness).Assembly)
                        {
                            if (!string.IsNullOrEmpty(prop.Value.ToString()))
                            {
                                var entity = Repository.Get(
                                    property.PropertyType.FullName,
                                    Convert.ToInt64(prop.Value));
                                property.SetValue(localBusiness, entity, null);
                            }
                            else
                            {
                                property.SetValue(localBusiness, null, null);
                            }
                        }
                        else //simple types
                        {
                            property.SetValue(localBusiness, prop.Value, null);
                        }
                    }
                }

                /* refresh access token for this page if requred */
                var accessToken =
                    Dror.Common.Utils.Facebook.GetPageAccessToken(
                         FacebookApp,
                         localBusiness.FanPageId,
                         CurrentUser.Id);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    localBusiness.FBFanPageAccessToken = accessToken;
                }

                Repository.Save(localBusiness);

                return BuildSuccessResult(0, null);
            }
            catch (Exception ex)
            {
                return BuildFailureResult(-1, ex.Message);
            }
        }

        public virtual JsonResult CheckProperty(long id, string propertyName, bool value)
        {
            var localBusiness = Repository.Get<LocalBusiness>(id);
            var rslt = IsAuthorized(localBusiness);
            if (rslt != null) return rslt;

            try
            {
                localBusiness.GetType()
                             .GetProperty(propertyName)
                             .SetValue(localBusiness, value, null);
                Repository.Save(localBusiness);

                return BuildSuccessResult(0,
                    App_GlobalResources.Strings.completedSucessfully);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check property", ex);
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.internalError);
            }
        }

        private JsonResult IsAuthorized(IEntity lb)
        {
            if (!MatchUserPrivilege(lb, Models.Common.UserPrivileges.Owner))
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }
            return null;
        }

        /// <summary>
        /// TOS page
        /// </summary>
        ///
        /// <returns>
        /// TOS view
        /// </returns>
        public virtual ActionResult TOS()
        {
            return View();
        }

        /// <summary>
        /// return users leaderboard
        /// </summary>
        ///
        /// <param name="localBussinesId">
        /// ID of the local business to get the board from
        /// </param>
        ///
        /// <returns>
        /// result of operation: array of UserDTO objects or view with list of
        /// users
        /// </returns>
        [EnableJson]
        public virtual PartialViewResult GetLeaders(long localBussinesId)
        {
            #region not supported yet by nhibernate
            //var aggregatedScore = from userInfo in Repository.Query<UserPlaylistInfo>()
            //                      where userInfo.Playlist.LocalBusiness.FanPageId == fanPageId
            //                      group userInfo by new { userInfo.Playlist.LocalBusiness, userInfo.User } into scoreInfo
            //                      select new
            //                      {
            //                          Groupings = scoreInfo.Key,
            //                          Points = scoreInfo.Sum(score => score.Points)
            //                      };

            //aggregatedScore = aggregatedScore
            //                        .OrderByDescending(score => score.Points)
            //                        .Take(LEADER_BOARD_SIZE);
            //var topUsers = aggregatedScore.Select(key => key.Groupings.User).ToList();
            //var topUsersScore = aggregatedScore.Select(key => key.Points).ToList();
            #endregion
            
            var lb = Repository.Get<LocalBusiness>(localBussinesId);

            //TODO: Get rid of these magic strings
            var usrPts = Repository.GropupBy(
                typeof(UserPlaylistInfo),
                null,
                "Playlist", lb.Playlists,
                "Points", "User", "grpPoints", LEADER_BOARD_SIZE);
       
            //TODO: get rid of magic indexes
            var usersPlaylistInfo = usrPts.Select(usrInfo => usrInfo[1] as FbUser).ToList();
            var usersScore = usrPts.Select(usrInfo => (int)usrInfo[0]).ToList();

            var membersDto = MemberDTOFactory.Map(usersPlaylistInfo, usersScore);
            return PartialView(MVC.Playlist.Views._Leaderboard, membersDto);
        }

        /// <summary>
        /// scheduled task handler: handler of periodic posts of updates to
        /// wall
        /// </summary>
        ///
        /// <returns>
        /// result of operation
        /// </returns>
        public virtual ActionResult PeriodicPost()
        {
            Logger.Info("Started periodic post");
            var lbs = Repository.Query<LocalBusiness>();

            foreach (var lb in lbs)
            {
                /* try posting to wall only if access token exists */
                if (String.IsNullOrEmpty(lb.FBFanPageAccessToken))
                {
                    var ownerId = (lb.Owners != null && lb.Owners.Count > 1) ?
                        lb.Owners[0].Id : 0;
                    if (lb.FBFanPageAccessToken == null)
                    {
                        continue;
                    }
                }

                /* facebook application used to post to the wall */
                var fbapp = new FacebookClient(lb.FBFanPageAccessToken);

                Logger.InfoFormat("Processing fan page: {0} ({1})",
                    lb.Name, lb.FanPageId);
                lb.Playlists.ToList().ForEach(
                    p => PostPlaylistUpdateToWall(fbapp, p));
            }
            return null;
        }

        /// <summary>
        /// Generate URL for the image of the post 
        /// </summary>
        /// <param name="rats"></param>
        /// <returns></returns>
        private static string GeneratePostImage(
            IList<PlaylistSongRating> rats)
        {
            if (rats == null)
            {
                return null;
            }
            if (!rats.Any())
            {
                return null;
            }

            /* if any of the songs has thumbnail, return it */
            var rat = rats.FirstOrDefault(
                r => (r.Song != null) &&
                    !String.IsNullOrEmpty(r.Song.ImageUrl));
            if (rat != null)
            {
                return rat.Song.ImageUrl;
            }

            /* find the first rating with a song */
            rat = rats.FirstOrDefault(
                r => (r.Song != null) &&
                    ! String.IsNullOrEmpty(r.Song.VideoID));
            if (rat == null)
            {
                /* nothing at all found, HUGE bug, use default image */
                return DEFAULT_PERIODIC_POST_IMAGE;
            }

            /* create YouTube thumbnail URL */
            return string.Format(YOUTUBE_THUMBNAIL_URL, rat.Song.VideoID);
        }

        /// <summary>
        /// post to wall update of a single 
        /// </summary>
        ///
        /// <param name="fbapp">
        /// Facebook application used to post to wall
        /// </param>
        ///
        /// <param name="pls">
        /// playlist to test
        /// </param>
        ///
        /// <returns>
        /// has this playlist posted to the wall?
        /// </returns>
        private void PostPlaylistUpdateToWall(
            FacebookClient fbapp, Playlist pls)
        {
            /* input sanity check */
            if (pls == null)
            {
                Logger.Error("Null playlist");
                throw new ArgumentNullException("pls");
            }

            /* push only the request posts */
            if (!pls.IsPushesToWall)
            {
                return;
            }

            Logger.InfoFormat(
                "Processing playlist {0} ({1}) from fan page {1}",
                pls.Name, pls.Id, pls.LocalBusiness.FanPageId);

            /* don't post to wall if the playlist already ended */
            if (pls.NextPlayDate < DateTime.Now)
            {
                Logger.InfoFormat("Playlist {0} already completed", pls.Id);
                return;
            }

            /* if the next update time has not yet arrived, discard the
             * request */
            if (DateTime.Now < pls.NextUpdate)
            {
                Logger.InfoFormat(
                    "Playlist {0} should not post to wall yet", pls.Id);
                return;
            }

            /* calculate next update time: set the default to 24 hours */
            var increment = (pls.IncrementUpdate >= 1) ?
                pls.IncrementUpdate : 24;
            pls.NextUpdate = DateTime.Now.AddHours(increment);
            Repository.SaveOrUpdate(pls);

            Logger.InfoFormat("Playlist {0} posting, next update: {1}",
                pls.Id, pls.NextUpdate);

            /* get names of the first N songs */
            var rats = pls
                .Ratings
                .OrderByDescending(r => r.SummedPositiveRating)
                .Take(PERIODIC_POST_SONGS);
            var ratsStr = rats
                .Select(r => string.Format(
                    "{0} - {1}", r.Song.Name, r.Song.ArtistName))
                .ToArray();
            if (ratsStr.Length < 1)
            {
                Logger.InfoFormat("Playlist {0} has no ratings", pls.Id);
                return;
            }

            var wholeMsg = new StringBuilder(
                pls.UpdateDescriptionFormat ??
                App_GlobalResources.Strings.periodicWallPost);
            for (var i = 0; i < ratsStr.Length; i++)
            {
                wholeMsg.AppendFormat("{0}. ", i + 1);
                wholeMsg.Append(ratsStr[i]);
                if (i < (ratsStr.Length - 1))
                {
                    wholeMsg.Append(',');
                }
            }
            var wholeMsgStr = wholeMsg.ToString();

            Logger.InfoFormat("Playlist {0} posting update to wall: {1}",
                pls.Id, wholeMsgStr);

            try
            {
                

                /* create the link to kululu application on current fan page */
                var link = string.Format(
                    "{0}?sk=app_{1}",
                    pls.LocalBusiness.FacebookUrl,
                    FacebookApp.GetAppId());

                var res = Dror.Common.Utils.Facebook.PostToWall(
                    fbapp,
                    pls.LocalBusiness.FanPageId,
                    "Update from Kulu.lu",
                    wholeMsgStr,
                    GeneratePostImage(rats.ToList()),
                    link,
                    string.Empty,    
                    string.Empty,
                    "link",
                    null);
                Logger.InfoFormat("Playlist {0} post to wall {1}",
                    pls.Id, !string.IsNullOrEmpty(res) ? "succeeded" : "failed");
            }
            catch (FacebookApiException e)
            {
                Logger.ErrorFormat(
                    "Playlist {0} failed to post to wall ({1}): {2}",
                    pls.Id, e.GetType(), e.Message);
            }
        }

        [EnableJson]
        public virtual ActionResult Settings(long id)
        {
            var localBusiness = Repository.Get<LocalBusiness>(id);

            var rslt = IsUserAllowedToEditEntity(localBusiness);
            if (rslt != null)
            {
                return rslt;
            }

            var pageDTO = BuildPageInfoDto(localBusiness, LoadStrings());

            return View(MVC.Settings.Views.Index, pageDTO);
        }

        /// <summary>
        /// model used by server status response
        /// </summary>
        public class ServerStatusModel
        {
            /// <summary>
            /// current status of the server "OK" or other
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// server request processing time
            /// </summary>
            public float ResponseTime { get; set; }
        }

        /// <summary>
        /// processor of server status
        /// </summary>
        ///
        /// <returns>
        /// view with the status of the server according to pingdom custom
        /// check format.
        /// </returns>
        ///
        /// <remarks>
        /// https://pp.pingdom.com/
        /// </remarks>
        public virtual ActionResult ServerStatusPingdom()
        {
            return View(MVC.LocalBusiness.Views.ServerStatusPingdom,
                new ServerStatusModel {Status = "OK", ResponseTime = 0.1f});
        }
    }
}

