using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Dror.Common.Utils;
using Kululu.Entities;
using Dror.Common.Data.Contracts;
using Kululu.Entities.Common;
using Facebook;
using Kululu.Web.Attributes;
using Kululu.Web.Models.Common;
using Dror.Common.Utils.Contracts;
using Kululu.Web.Common;
using Kululu.Web.Facebook;
using System.Web.Routing;
using Facebook.Web;
using System.Web;
using FbUtils = Dror.Common.Utils.Facebook;

namespace Kululu.Web.Controllers
{
    
    public partial class AccountController : BaseController
    {
        /// <summary>
        /// default like page image
        /// </summary>
        private const string DEFAULT_LIKE_PAGE_URL =
            "/Content/img/like_world.jpg";

        /// <summary>
        /// maximal number of results to return when retrieving user votes
        /// </summary>
        private const int MAX_RETURNED_VOTES = 20;

        /// <summary>
        /// separator of locale and specific language in locale field of signed
        /// request
        /// </summary>
        private const char FB_LOCALE_SEPARATOR = '_';

        //1. Retrun the user's login status
        //2. Return user's authorization level
        public AccountController(IRepository repository, FacebookClient fbApp,
            ILogger logger) : base(repository, fbApp, logger)
        {
        }

        #region Facebook Callbacks

        public virtual ActionResult Welcome()
        {
            return Redirect("http://kulu.lu");
        }

        public virtual ActionResult Connect()
        {
            return Start();
            //TODO: uncomment this line, support preloader
            //return View(MVC.Shared.Views.Perloader,
            //    model: HttpContext.Request["signed_request"]);
        }

        private ActionResult RedirectToPlaylistByFanPage(long fanPageId)
        {
           return RedirectToAction(
                "GetPlaylistByFanPage",
                "LocalBusiness",
                new RouteValueDictionary(new { fanPageId }));
        }

        [EnableJson]
        public virtual ActionResult Index(string name)
        {
            return null;
        }

        [EnableJson]
        public virtual ActionResult Start()
        {
            if (!IsBrowserSupported())
            {
                return View(MVC.Shared.Views.BrowserNotSupported);
            }

            if (Request.Browser.IsMobileDevice)
            {
                //return null;
                //return View(MVC.Mobile.Views.PlaylistMobile, playlistsDTO);
            }

            /* set default values matching localhost application */
            /* fan page ID used to pull playlists */
            var fanPageId = 258829897475960;

            /* checking to see if working in localhost enviroment, if there is
             * some failure in retrieving facebook data, default values will
             * be used */
            var signedRequest = HttpContext.Request["signed_request"];

            /* parse the signed_request if it exists */
            var fbSession = (signedRequest != null) ?
                ExtractSessionData(signedRequest) : null;

            if (signedRequest == null)
            {
                return RedirectToPlaylistByFanPage(fanPageId);
            }

            if (fbSession == null)
            {
                return View(MVC.Shared.Views.Error, new ErrorMessage(
                    App_GlobalResources.Errors.invalidFacebookSession));
            }

            //fbSession has a value therefore it means that user has accessed through page
            CurrentUserFBInfo.IsCanvas = false;

            /* if kululu user logged off his facebook account, or changed his
             * facebook user */
            if (CurrentUserFBInfo != null &&
                CurrentUserFBInfo.FacebookId != fbSession.UserId)
            {
                CurrentUserFBInfo = null;
            }

            /* if user has already approved app, set his id now */
            if (fbSession.UserId != 0)
            {
                CurrentUserFBInfo.FacebookId = fbSession.UserId;
            }

            CurrentUserFBInfo.IsAdmin = fbSession.IsAdmin;
            CurrentUserFBInfo.HasLikedPage = fbSession.IsLiked;
            CurrentUserFBInfo.AccessToken = fbSession.Token;
            
            fanPageId = fbSession.PageId;

            var localBusiness = Repository
                .Query<LocalBusiness>()
                .FirstOrDefault(lb => lb.FanPageId == fanPageId);

            // no local business exists
            if (localBusiness == null)
            {
                // we only show the create page to admin users
                return fbSession.IsAdmin ?
                    RedirectToCreate(fanPageId) :
                    View(MVC.Shared.Views.ComeBackLater);
            }

            /* need to make sure page admin is also localbusiness owner and
             * that has authorized the app */
            if (CurrentUserFBInfo.IsAdmin && CurrentUserFBInfo.FacebookId != 0)
            {
                // saving admin if he's not saved already
                var admin = CreateUser(CurrentUserFBInfo.FacebookId,
                    UserStatus.Pending);
                if (admin != null && !localBusiness.IsOwner(admin.Id))
                {
                    localBusiness.AddOwner(admin);
                    Repository.SaveOrUpdate(localBusiness);
                }
            }

            /* if the like for the page is required to see the page and the
             * user is not administrator, redirect to "Like" page */
            if (!CurrentUserFBInfo.IsAdmin &&
                !CurrentUserFBInfo.HasLikedPage &&
                localBusiness.IsLikeDemanded)
            {
                var likeImage = UrlUtils.BuildApplicationImageUrl(
                    localBusiness.LikePageImage,
                    HttpContext.Request.ApplicationPath,
                    DEFAULT_LIKE_PAGE_URL);
                return View(MVC.Account.Views.JoinUs, model: likeImage);
            }

            var encodedAppData = HttpUtility.UrlDecode(fbSession.AppData);
            // getting the referring playlist if there is one
            long referringPlaylistId = 0;
            long referringPlaylisingSongRating = 0;

            if (encodedAppData != null)
            {
                var decodedAppData =
                    HttpUtility.ParseQueryString(encodedAppData);

                var playlistSongRatingId =
                    decodedAppData["playlistSongRatingId"];
                var playlistId = decodedAppData["playlistId"];

                if (!string.IsNullOrEmpty(playlistSongRatingId))
                {
                    referringPlaylisingSongRating =
                        long.Parse(playlistSongRatingId);
                }

                if (!string.IsNullOrEmpty(playlistId))
                {
                    referringPlaylistId = long.Parse(playlistId);
                }
            }

            var playlist = (referringPlaylistId == 0) ?
                localBusiness.DefaultPlaylist :
                localBusiness.Playlists.FirstOrDefault(
                    p => p.Id == referringPlaylistId);

            UpdateLocale(fbSession.Locale, playlist);

            return ShowDefaultScreen(playlist, referringPlaylisingSongRating);
        }

        /// <summary>
        /// select required locale based on current user preferences, current
        /// playlist preferences and facebook user preferences
        /// </summary>
        ///
        /// <param name="pls">
        /// Playlist currently being viewed by the user
        /// </param>
        ///
        /// <param name="fbLocale">
        /// locale string from Facebook signed_user
        /// </param>
        ///
        /// <returns>
        /// string with required user locale
        /// </returns>
        private string SelectRequiredLocale(Playlist pls, string fbLocale)
        {
            /* possible values for locale names */
            var validLocales = Enum.GetNames(typeof (MvcApplication.Culture));

            if (CurrentUser != null &&
                !String.IsNullOrEmpty(CurrentUser.Locale) &&
                validLocales.Contains(CurrentUser.Locale))
            {
                return CurrentUser.Locale;
            }

            /* select customization based on the following preference:
             * 1. with both local business and playlsit set.
             * 2. with only playlist set.
             * 3. with only local business set. */
            if (pls != null && pls.LocalBusiness != null)
            {
                var cust =
                    Repository
                    .Query<Customizations>()
                    .Where(c => c.Playlist == pls)
                    .Where(c => c.LocalBusiness == pls.LocalBusiness)
                    .Where(c => c.Locale != null)
                    .Where(c => !c.Locale.Equals(string.Empty))
                    .FirstOrDefault() ??

                    Repository
                    .Query<Customizations>()
                    .Where(c => c.Playlist == pls)
                    .Where(c => c.Locale != null)
                    .Where(c => !c.Locale.Equals(string.Empty))
                    .FirstOrDefault() ??

                    Repository
                    .Query<Customizations>()
                    .Where(c => c.LocalBusiness == pls.LocalBusiness)
                    .Where(c => c.Locale != null)
                    .Where(c => !c.Locale.Equals(string.Empty))
                    .FirstOrDefault();

                if (cust != null && validLocales.Contains(cust.Locale))
                {
                    return cust.Locale;
                }
            }

            /* trim the "he_IL" notation to simple "he" */
            var tfbl = string.IsNullOrEmpty(fbLocale) ?
                null : fbLocale.Split(FB_LOCALE_SEPARATOR)[0];

            if (!String.IsNullOrEmpty(tfbl) && validLocales.Contains(tfbl))
            {
                return tfbl;
            }
            return Thread.CurrentThread.CurrentCulture.Name;
        }

        /// <summary>
        /// updates locale of the current session
        /// </summary>
        ///
        /// <param name="fbLocale">
        /// Facebook locale as received from signed request
        /// </param>
        ///
        /// <param name="pls">
        /// playlist from which locale should be pulled
        /// </param>
        private void UpdateLocale(string fbLocale, Playlist pls)
        {
            /* if the session does not exist yet, bail out */
            if (HttpContext.Session == null)
            {
                return;
            }

            /* ensure the locale is set correctly */
            var requiredLocale = SelectRequiredLocale(pls, fbLocale);
            var cu = (CultureInfo)
                HttpContext.Session[SessionVariables.CULTURE];
            if (cu != null && cu.Name.Equals(requiredLocale))
            {
                /* if the currently set locale matches the required one, do
                 * nothing */
                return;
            }

            cu = new CultureInfo(requiredLocale);
            HttpContext.Session[SessionVariables.CULTURE] = cu;

            /* Finally setting culture for each request */
            Thread.CurrentThread.CurrentUICulture = cu;
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(cu.Name);
        }

        [RequireRequestValueAttribute(new[] { "playlistId" })]
        public virtual ActionResult ShowDefaultScreen(long playlistId)
        {
            var playlist = Repository.Get<Playlist>(playlistId);
            return ShowDefaultScreen(playlist, 0);
        }

        private ActionResult ShowDefaultScreen(Playlist playlist,
            long referringPlaylisingSongRating)
        {
            var pageInfoDTO =
                BuildPageInfoDto(playlist.LocalBusiness, LoadStrings());

            pageInfoDTO.ReferringPlaylisingSongRating =
                referringPlaylisingSongRating;

            pageInfoDTO.Strings = LoadStrings();

            return View(MVC.Playlist.Views.Playlist, pageInfoDTO);
        }

        public virtual void RemoveApp()
        {
            var signedRequest = HttpContext.Request["signed_request"];
            if (signedRequest == null)
            {
                return;
            }

            var fbSession = ExtractSessionData(signedRequest);
            
            if (fbSession == null)
            {
                return;
            }

            var user = Repository.Get<FbUser>(fbSession.UserId);
            user.Status = UserStatus.Removed;
            Repository.SaveOrUpdate(user);
            Logout();
        }

        /// <summary>
        /// parse signed_request object received from Facebook into session
        /// object
        /// </summary>
        ///
        /// <param name="signedRequest">
        /// signed request as received from Facebook
        /// </param>
        ///
        /// <returns>
        /// parsed FacebookSession object of null on failure
        /// </returns>
        private static FbmlFacebookSession ExtractSessionData(
            string signedRequest)
        {
            var fbApp = new FacebookWebClient();
            var fbSignedRequest = new FBSignedRequestManager();
            return fbSignedRequest.ParseSignedRequest(signedRequest,
                fbApp.GetAppSecret());
        }

        private ActionResult RedirectToCreate(long fanPageId)
        {
            return RedirectToAction(
                 "Create",
                 "Playlist",
                 new RouteValueDictionary(new { fanPageId }));
        }
        #endregion

        #region Browser Support
        private readonly BrowserInfo[] UnsupportedBrowsers = new[]
        {
           new BrowserInfo(BrowserName.IE, 6)
           //new BrowserInfo(BrowserName.IE, 7)
        };

        /// <summary>
        /// check for supported browsers
        /// </summary>
        ///
        /// <returns>
        /// true if current user's browser is supported, false otherwise
        /// </returns>
        private bool IsBrowserSupported()
        {
            BrowserName browserName;

            double browserVersion;

            if (!double.TryParse(Request.Browser.Version, out browserVersion))
            {
                return false;
            }

            if (Enum.TryParse(Request.Browser.Browser, out browserName))
            {
                /* browser type comparison result */
                var unsupBrowser = UnsupportedBrowsers.Any(
                    browser => browser.Name == browserName
                        && browser.Version == browserVersion);
                if (unsupBrowser)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Login

        /// <summary>
        ///does nothing right now, think of an implementation if needed 
        /// </summary>
        /// <returns></returns>
        public virtual bool Logout()
        {
            try
            {
                CurrentUserFBInfo = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        [RequireRequestValueAttribute(new[] { "accessToken", "playlistId" })]
        public virtual JsonResult Login(string accessToken, long? playlistId)
        {
            /* current user privileges */
            UserPrivileges privileges;

            /* playlist to operate on */
            Playlist playlist = null;
            if (playlistId != null)
            {
                playlist = Repository.Get<Playlist>(playlistId);
            }

            /* ensure that current user is logged into facebook and into
             * application */
            var res = LoginInternal(accessToken, UserStatus.Joined, playlist);
            if (res == false)
            {
                return BuildFailureResult(
                    -1, App_GlobalResources.Errors.invalidUser);
            }

            if (CurrentUser == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.invalidUser);
                //TODO: handle some error code here
            }

            /* if playlist was specified and it exists in the database, extract
             * number or remaiming songs for the user in the database,
             * otherwise just reset the value to zero */
            
            /* number of songs left for current user to add */
            if (playlist != null)
            {
                playlist.GetNumOfSongsLeft(CurrentUser);
                privileges = GetCurrentUserPrivileges(playlist);
            }
            else
            {
                privileges = UserPrivileges.None;
            }

            var userDto = BuildUserDTO(playlist, privileges);
            
            SaveUserVisit(playlist, CurrentUser);
            return BuildSuccessResult(1, userDto);
        }

        private void SaveUserVisit(Playlist playlist, FbUser user)
        {
            var visit= new Visit{ Playlist = playlist, User = user, VisitDate = DateTime.Now};
            Repository.Save(visit);
        }

        /// <summary>
        /// login operations
        /// </summary>
        ///
        /// <param name="accessToken">
        /// Facebook access token
        /// </param>
        ///
        /// <param name="userStatus">
        /// current state of the user
        /// </param>
        ///
        /// <param name="playlist">
        /// playlist being loged into
        /// </param>
        ///
        /// <returns>
        /// true if the user logged in successfully, false otherwise
        /// </returns>
        private bool LoginInternal(string accessToken, UserStatus userStatus,
            Playlist playlist = null)
        {
            CurrentUserFBInfo.AccessToken = accessToken;
            FacebookApp = (accessToken == null) ?
                new FacebookWebClient() : new FacebookWebClient(accessToken);

            var fbUser = GetLoggedInFbUser();
            
            var userId = long.Parse(fbUser.id);
            var user = SetLoggedInUser(userId);

            /* in case this is a user who approved the app back in the time we
             * didn't request email permission */
            if (user == null ||
                CurrentUser.Status == UserStatus.Pending ||
                CurrentUser.Status == UserStatus.Removed ||
                string.IsNullOrEmpty(CurrentUser.Email))
            {
                if (!SaveUser(fbUser, userStatus))
                {
                    return false;
                }
                
                user = SetLoggedInUser(userId);
                if (user == null)
                {
                    return false;
                }

                if (playlist != null)
                {
                    RewardUser(playlist, UserPlaylistInfo.Operation.Signup);
                }
            }
            else
            {
                RewardUser(playlist, UserPlaylistInfo.Operation.DayEntrance);
            }
            return true;
        }

        private FbUser SetLoggedInUser(long id)
        {
            var currentUser =
                Repository.Get<FbUser>(id);

            CurrentUserFBInfo.FacebookId = currentUser !=null 
                ? CurrentUserFBInfo.FacebookId = currentUser.Id
                : CurrentUserFBInfo.FacebookId;

            return CurrentUser;
        }

        /// <summary>
        /// retrieve information about currently logged in Facebook user
        /// </summary>
        ///
        /// <returns>
        /// Facebook object with information about currently logged in user as
        /// returned by Facebook
        /// </returns>
        private dynamic GetLoggedInFbUser()
        {
            return (FacebookApp.AccessToken == null) ?
                null : FacebookApp.Get("/me", null);
        }

        private FbUser CreateUser(long userId, UserStatus status)
        {
            var usr = Repository.Get<FbUser>(userId);
            if (usr == null)
            {
                usr = new FbUser
                    {
                        Id = userId,
                        JoinDate = DateTime.Now,
                        Status = status
                    };
                Repository.Save(usr);
            }
            else if(usr.Status == UserStatus.Removed)
            {
                usr.Status = status;
                Repository.Update(usr);
            }
            return usr;
        }

        private bool SaveUser(dynamic user, UserStatus userStatus)
        {
            if (user == null)
            {
                return false;
            }
            
            long userId;
            long.TryParse(user.id, out userId);

            var fbUser = Repository.Get<FbUser>(userId);
            try
            {
                if (fbUser == null)
                {
                    fbUser = new FbUser {Id = userId};
                }

                fbUser.Status = userStatus;
                fbUser.Name = user.name;
                fbUser.ProfileImageUrl = user.profile_imageUrl; //TODO: Find real profile image url
                fbUser.RelationshipStatus = user.relationship_status;
                //fbUser.SignificantOther =
                //    ExtractSignificantOther(user.significant_other);
                fbUser.JoinDate = DateTime.Now;
                fbUser.LinkToProfile = user.link;
                fbUser.Gender = user.gender;
                fbUser.FullName = user.name;
                fbUser.Email = user.email;
                Repository.SaveOrUpdate(fbUser);
            }
            catch (Exception)
            {
                /* Prompt user to try again as he wasn't saved correctly */
                return false;
            }
            return true;
        }

        #endregion

        #region Future Usages
        
        public virtual JsonResult RecentActivity(int offset, int amount)
        {
            /* if the user is not logged in, get out */
            if (CurrentUser == null)
            {
                return null;
            }

            /* limit maximal allowed number of results */
            var maxAmount = Math.Max(offset, MAX_RETURNED_VOTES);

            /* songs user added */
            var additions = Repository.Query<PlaylistSongRating>()
                .Where(r => r.Creator.Id == CurrentUser.Id)
                .OrderByDescending(r => r.CreationTime)
                .Skip(offset)
                .Take(maxAmount)
                .Select(r => new
                    {
                        SongId = r.Song.Id,
                        SongName = r.Song.Name,
                        Date = r.CreationTime
                    });

            /* songs user voted on */
            var votes = Repository.Query<RatingDetails>()
                .Where(p => p.VotingUser.Id == CurrentUser.Id)
                .OrderByDescending(r => r.LastUpdated)
                .Skip(offset)
                .Take(maxAmount)
                .Select(r => new
                    {
                        SongId = r.PlaylistSongRating.Song.Id,
                        SongName = r.PlaylistSongRating.Song.Name,
                        PlaylistId = r.PlaylistSongRating.Playlist.Id,
                        PlaylistName = r.PlaylistSongRating.Playlist.Name,
                        PlaylistDescription = r.PlaylistSongRating.Playlist.Description,
                        Date = r.LastUpdated
                    });

            return Json(new
                {
                    Additions = additions,
                    Votes = votes,
                });
        }
        #endregion

        /// <summary>
        /// save single user email into table
        /// </summary>
        ///
        /// <param name="email">
        /// email to be saved into the database
        /// </param>
        ///
        /// <returns>
        /// true if the email was saved successfully into the database, false
        /// otherwise
        /// </returns>
        public virtual bool Soon(string email)
        {
            var eml = Repository.Get<Emails>(email);
            if (eml == null)
            {
                eml = new Emails {Email = email};
                Repository.Save(eml);
            }
            return true;
        }

        /// <summary>
        /// create test user (tesing purposes)
        /// </summary>
        ///
        /// <returns>
        /// JSON object returned by FB API
        /// </returns>
        public virtual ActionResult CreateTestUser()
        {
            if (CurrentUser == null)
            {
                return View(MVC.Shared.Views.Error,
                    new ErrorMessage(App_GlobalResources.Errors.accessDenied));
            }
            if (CurrentUser.Id != 664118894 &&
                CurrentUser.Id != 716712905 &&
                CurrentUser.Id != 662326561)
            {
            }
            var tup = new Dictionary<string, object>
                {
                    {"installed", true},
                    {"permissions", "read_stream"},
                    {"access_token", string.Format("{0}|{1}", FacebookApp.GetAppId(), FacebookApp.GetAppSecret())}
                };
            try
            {
                dynamic result = FacebookApp.Post(
                    string.Format("{0}/accounts/test-users",
                        FacebookApp.GetAppId()),
                    tup);
                return View(MVC.Account.Views.TestUser, result);
            }
            catch (FacebookApiException e)
            {
                return View(MVC.Shared.Views.Error, e.Message);
            }
        }

        /// <summary>
        /// change the language used by this user and by this session
        /// </summary>
        ///
        /// <param name="lang">
        /// language name, one of supported locales
        /// </param>
        ///
        /// <param name="returnUrl">
        /// URL to redirect to return to after changing the locale
        /// </param>
        ///
        /// <returns>
        /// redirect to the new page
        /// </returns>
        public virtual ActionResult ChangeCulture(string lang,
            string returnUrl)
        {
            var cults = Enum.GetNames(typeof(MvcApplication.Culture));
            if (!cults.Contains(lang))
            {
                return Redirect(returnUrl);
            }

            if (CurrentUser != null)
            {
                CurrentUser.Locale = lang;
                Repository.SaveOrUpdate(CurrentUser);
            }

            UpdateLocale(lang, null);
            return Redirect(returnUrl);
        }
    }
}
