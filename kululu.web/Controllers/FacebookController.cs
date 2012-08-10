using System;
using System.Linq;
using System.Web.Mvc;
using Dror.Common.Data.Contracts;
using Dror.Common.Utils;
using Facebook;
using Dror.Common.Utils.Contracts;
using FbUtils = Dror.Common.Utils.Facebook;
using Kululu.Entities;
using System.Web.Script.Serialization;
using Kululu.Web.Models.Common;
using System.Text;
using System.Web;
using System.Collections.Generic;
using Kululu.Web.Models;

namespace Kululu.Web.Controllers
{
    public partial class FacebookController : BaseController
    {
        /// <summary>
        /// default image for feedback posting
        /// </summary>
        private const string DEFAULT_FEEDBACK_POST_IMAGE =
            "Content/img/default_feedback.png";

        public FacebookController(IRepository repository, FacebookClient fbApp, ILogger logger)
            : base(repository, fbApp, logger)
        {

        }

        #region Graph
        public virtual ActionResult SongInfo(long id)
        {
            var pSongRating= Repository.Get<PlaylistSongRating>(id);
            var pSongRatingDTO = SongWithUserRatingDtoFactory.Map(pSongRating);
            return View(MVC.FbGraph.Views.SongInfo, pSongRatingDTO);
        }
        #endregion

        //
        // GET: /Facebook/

        public bool SetFbLike(long playlistId, long playlistSongRatingId, short rating)
        {
            if (!CurrentUserFBInfo.IsAdmin && !CurrentUserFBInfo.HasLikedPage)
            {
                return false;
            }

            var playlist = Repository.Get<Playlist>(playlistId);
            if (playlist == null)
            {
                return false;
            }

            var ratingInfo = playlist.Ratings.FirstOrDefault(ratings => ratings.Id == playlistSongRatingId);
            if (ratingInfo == null)
            {
                return false;
            }

            if (ratingInfo.FBPostId == null)
            {
                return true;
            }

            var localBusiness = ratingInfo.Playlist.LocalBusiness;
            return FbUtils.SetFbLike(ratingInfo.FBPostId, rating, FacebookApp);
        }

        public bool RemovePost(long playlistId, long playlistSongRatingId)
        {
            if (!CurrentUserFBInfo.IsAdmin && !CurrentUserFBInfo.HasLikedPage)
            {
                return false;
            }

            var playlist = Repository.Get<Playlist>(playlistId);
            if (playlist == null)
            {
                return false;
            }

            var playlistSongRating = playlist.Ratings.FirstOrDefault(ratings => ratings.Id == playlistSongRatingId);

            if (playlistSongRating == null || string.IsNullOrEmpty(playlistSongRating.FBPostId))
            {
                return false;
            }

            var allowedUsers = MatchUserPrivilege(playlistSongRating, UserPrivileges.Owner);
            if (!allowedUsers)
            {
                return false;
            }

            return FbUtils.RemovePost(playlistSongRating.FBPostId, FacebookApp);
        }

        public bool PostToWall(
            long playlistID,
            long playlistSongRatingId,
            string msg,
            string description = null)
        {
            var playlist = Repository.Get<Playlist>(playlistID);
            var localBusiness = playlist.LocalBusiness;
            var ratingInfo = playlist.Ratings.FirstOrDefault(
                rating => rating.Id == playlistSongRatingId);
            var isAdmin = CurrentUserFBInfo.IsAdmin;

            if (!isAdmin && !CurrentUserFBInfo.HasLikedPage)
            {
                //user can't post because she hasn't like the page
                return false;
            }

            if (ratingInfo != null && !string.IsNullOrEmpty(ratingInfo.FBPostId))
            {
                // Facebook Post already exists for this song
                return false;
            }

            //if (!FacebookApp.IsAuthenticated)
            //{
            //    return false; // only post to the wall the app is authenticated
            //}

            if (!localBusiness.PublishUserContentToWall &&
                !localBusiness.PublishAdminContentToWall)
            {
                // no publish is allowed
                return false;
            }

            if (!isAdmin && !localBusiness.PublishUserContentToWall)
            {
                // if current user is no admin, and is not allowed to post
                return false;
            }

            if (isAdmin && !localBusiness.PublishAdminContentToWall)
            {
                // if user is admin, and not allowed to post in settings
                return false;
            }

            FacebookClient fbApp = null;

            /* if you are a page admin and page settings allow you to post
             * (only as admin!)*/
            if (isAdmin && localBusiness.PublishAdminContentToWall)
            {
                var accessToken = FbUtils.GetPageAccessToken(
                    FacebookApp, localBusiness.FanPageId, CurrentUser.Id);
                fbApp = new FacebookClient(accessToken);
            }
            else if (!isAdmin || !localBusiness.PublishAdminContentToWall)
            {
                /* if you are no admin, or page settings don't allow admins to
                 * publish in page's name */
                fbApp = FacebookApp;
            }

            if (fbApp == null)
            {
                return false;
            }

            FbUtils.PostToWall(fbApp, localBusiness.FanPageId, null, null,
                null, null, null, null, null, null);
            
            var songName = string.Format(" {0} ", ratingInfo.Song.Name);
            var name = string.IsNullOrEmpty(ratingInfo.Song.ArtistName) ?
                songName :
                string.Format("{0} - {1}", ratingInfo.Song.ArtistName, songName);
            var msgF = string.Format("{0} {1}",
                Web.Views.Playlist.Playlist.DialogueTemplatePost_Content, 
                playlist.Name);
            var message = msg == "" ? msgF : msg.Replace("\n", "  ");
            
        
            var descriptionStr = string.IsNullOrEmpty(description) ?
                Web.Views.Playlist.Playlist.DialogueTemplatePost_Description
                : description;

            string postType;
            var picture = string.Format("http://img.youtube.com/vi/{0}/0.jpg",
                ratingInfo.Song.VideoID);
            string postId;

            var appData = HttpUtility.UrlEncode(
                string.Format("appData=playlistId={0}&playlistSongRatingId={1}",
                playlistID,
                playlistSongRatingId));

            var link = FbUtils.GetLinkToApp(
                localBusiness.Name.Replace(" ", "-"),
                localBusiness.FanPageId,
                FacebookApp.GetAppId(),
                appData);

            var properties = new Dictionary<string, string>
                {
                    {   
                        App_GlobalResources.Strings.kululu, 
                        Web.Views.Playlist.Playlist.musicWeLike
                    }
                };

            if (isAdmin || localBusiness.UserPostToWallType == Entities.Common.FbPostToWallType.Video)
            {
                var source = string.Format(
                    "http://www.youtube.com/e/{0}?autoplay=1",
                    ratingInfo.Song.VideoID);
                postType = "video";
                postId = FbUtils.PostToWall(
                    fbApp,
                    localBusiness.FanPageId,
                    name,
                    message,
                    picture,
                    link,
                    source,
                    descriptionStr,
                    postType,
                    properties);
            }
            else
            {
                postType = "post";
                postId = FbUtils.PostToWall(
                    fbApp,
                    localBusiness.FanPageId,
                    name,
                    message,
                    picture,
                    link,
                    picture,
                    descriptionStr,
                    postType,
                    properties);
            }
          
            ratingInfo.FBPostId = postId;
            ratingInfo.FBMessage = msg;
            ratingInfo.FBDescription = descriptionStr;
            Repository.Update(ratingInfo);
            SetFbLike(playlist.Id, ratingInfo.Id, 1);

            /* send the email to administrator about posting to wall */
            if (isAdmin &&
                !string.IsNullOrEmpty(localBusiness.EmailOnAdminPost))
            {
                SendEmail(localBusiness, songName);
            }

            return true;
        }

        /// <summary>
        /// send an email with feedback to us (kululu)
        /// </summary>
        ///
        /// <param name="feedback">
        /// contents of the feedback posted
        /// </param>
        ///
        /// <param name="playlistId">
        /// ID of the playlist from which the feedback is sent
        /// </param>
        ///
        /// <returns>
        /// result of operation
        /// </returns>
        [HttpPost]
        public virtual ActionResult SendFeedback(
            string feedback,
            long playlistId = 0)
        {
            /* was the email sent? */
            bool isEmailSent;

            /* post to wall result */
            string postResult = null;

            /* helper for building feedback email message */
            var body = new StringBuilder();

            /* image URL for the post on the wall */
            string imgUrl;

            /* URL of the post on the wall */
            string url;

            body.AppendFormat(
                "<span style='font-weight:bold'> Mail from: </span>  <br/>  name: {0} </t> id: {1} ",
                (CurrentUser == null) ? string.Empty : CurrentUser.Name,
                (CurrentUser == null) ? 0 : CurrentUser.Id);

            body.AppendFormat(
                "<span style='font-weight:bold'> Content: </span> <br/> {0}",
                feedback);

            var eml = new Email(
                App_GlobalResources.Strings.adminEmailSentFromAddress,
                App_GlobalResources.Strings.adminEmailSentFromName,
                App_GlobalResources.Strings.adminEmailSentFromAddress,
                App_GlobalResources.Strings.adminEmailSentFromName,
                App_GlobalResources.Strings.feedbackTitle,
                body.ToString());

            isEmailSent = eml.Send();
            if (!isEmailSent)
            {
                /* failed to send email */
                Logger.WarnFormat("Failed to email feedback: {0}", feedback);
            }

            var pls = Repository.Get<Playlist>(playlistId);
            var plsFeedback = String.Format(
                App_GlobalResources.Strings.FeedbackForPlaylist,
                (pls != null) ? pls.Name : string.Empty);
            

            if (pls != null)
            {
                url = string.Format("{0}?sk=app_{1}",
                    pls.LocalBusiness.FacebookUrl,
                    FacebookApp.GetAppId());
                imgUrl = pls.LocalBusiness.ImageUrl;
            }
            else
            {
                imgUrl = UrlUtils.BuildAbsoluteImageUrl(
                    HttpContext.Request.Url,
                    DEFAULT_FEEDBACK_POST_IMAGE,
                    string.Empty,
                    string.Empty);
                url = null;
            }

            try
            {
                postResult = Dror.Common.Utils.Facebook.PostToWall(
                    FacebookApp,
                    Dror.Common.Utils.Facebook.APP_FAN_PAGE_ID,
                    plsFeedback,
                    feedback,
                    imgUrl,
                    url,
                    null,
                    null,
                    Dror.Common.Utils.Facebook.WallPostType.post.ToString(),
                    null);
            }
            catch (FacebookApiException e)
            {
                Logger.Error("Failed to post to wall", e);
            }

            if (!isEmailSent || string.IsNullOrEmpty(postResult))
            {
                var result = string.Format(
                    "Failed to post feedback: Email: {0}. Wall: {1}",
                    isEmailSent, postResult);
                BuildFailureResult(-1, result);
            }

            return BuildSuccessResult(0, null);
        }

        /// <summary>
        /// Send email to administrator of the page about posting to wall
        /// </summary>
        ///
        /// <param name="localBusiness">
        /// local business for which the email should be sent
        /// </param>
        ///
        /// <param name="songName">
        /// name of the song posted to the wall
        /// </param>
        private void SendEmail(LocalBusiness localBusiness, string songName)
        {
            var body = String.Format(
                    App_GlobalResources.Strings.adminEmailSentBody,
                    songName);
            var eml = new Email(
                localBusiness.EmailOnAdminPost,
                localBusiness.Owners[0].FullName,
                App_GlobalResources.Strings.adminEmailSentFromAddress,
                App_GlobalResources.Strings.adminEmailSentFromName,
                App_GlobalResources.Strings.adminEmailSentSubject,
                body);
            if (eml.Send())
            {
                Logger.InfoFormat("Sent email to administrator");
            }
            else
            {
                Logger.WarnFormat(
                    "Failed to send email to administrator of {0} ({1})",
                    localBusiness.Name, localBusiness.FanPageId);
            }
        }

        /// <summary>
        /// test if the specified users are admins of the fan page
        /// </summary>
        ///
        /// <param name="localbusinessId">
        /// local business ID
        /// </param>
        /// 
        /// <param name="userIds">
        /// list of users to be tested (JSON formatted array)
        /// </param>
        ///
        /// <returns>
        /// standard response with dictionary of users and their corresponding
        /// role: admin or not
        /// </returns>
        public virtual JsonResult ArePageAdmins(long localbusinessId,
            string userIds)
        {
            var localBusiness = Repository.Get<LocalBusiness>(localbusinessId);
            if (!localBusiness.IsOwner(CurrentUser.Id))
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }

            var accessToken = localBusiness.FBFanPageAccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.facebookAccessTokenNotStored);
            }

            var fbApp = new FacebookClient(accessToken);

            var serializer = new JavaScriptSerializer();
            var users = serializer.Deserialize<string[]>(userIds);
            var rslt = Dror.Common.Utils.Facebook.AreAdmins(
                fbApp,
                localBusiness.FanPageId,
                users,
                Logger);


            return BuildSuccessResult(1, rslt);

        }
    }
}