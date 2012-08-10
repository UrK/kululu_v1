using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Web.Mvc;
using Kululu.Entities;
using Facebook;
using Dror.Common.Data.Contracts;
using Kululu.Web.Models;
using Kululu.Web.Models.Common;
using Dror.Common.Utils.Contracts;
using Kululu.Web.Startup;
using Facebook.Web;
using System.Configuration;

namespace Kululu.Web.Controllers
{
    public abstract partial class BaseController : Controller
    {
        /// <summary>
        /// embedded resources names that will be rendered globally into
        /// HTML code
        /// </summary>
        private static readonly string[] MResourcesNames = new string[]
        {
            "Kululu.Web.Views.Shared.Shared",
            "Kululu.Web.Views.Playlist.Playlist"
        };

        protected ILogger Logger { get; set; }
        protected IRepository Repository { get; set;}

        FacebookClient m_facebookApp;
        protected FacebookClient FacebookApp
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentUserFBInfo.AccessToken))
                {
                    var config =
                        (ConfigurationManager.GetSection("facebookSettings") as FacebookConfigurationSection);
                        m_facebookApp = new FacebookWebClient(config.AppId, config.AppSecret);
                }
                else
                {
                    m_facebookApp = new FacebookWebClient(
                        CurrentUserFBInfo.AccessToken);
                }
                return m_facebookApp;
            }
            set
            {
                m_facebookApp = value;
            }
        }

        protected object SessionLock
        {
            get
            {
                return Session["SessionLock"] ??
                    (Session["SessionLock"] = new object());
            }
            set
            {
                Session["SessionLock"] = value;
            }
        }

        protected BaseController(IRepository repository, FacebookClient app,
            ILogger logger)
        {
            Repository = repository;
            FacebookApp = app;
            Logger = logger;
        }

        /// <summary>
        /// compare user privileges
        /// </summary>
        ///
        /// <param name="current">
        /// level of access of current user
        /// </param>
        ///
        /// <param name="required">
        /// required level of access
        /// </param>
        ///
        /// <returns>
        /// true if there is a match, false otherwise
        /// </returns>
        ///
        /// <remarks>
        /// This method should be used instead of simple comparison. This is
        /// required for future expansion, for example, administrator of the
        /// system is always a regular user. someday we may decide that if
        /// current user access level is administrator and required level is
        /// user,  we may return return "true" too.
        /// </remarks>
        protected bool MatchUserPrivilege(UserPrivileges current,
            UserPrivileges required)
        {
            return current == required;
        }

        /// <summary>
        /// see if current user has sufficient access to specified object
        /// </summary>
        ///
        /// <param name="entity">
        /// object to check for access to
        /// </param>
        ///
        /// <param name="requiredPrivileges">
        /// required access level to specified entity
        /// </param>
        ///
        /// <returns>
        /// true if there is a match, false otherwise
        /// </returns>
        protected bool MatchUserPrivilege(IEntity entity,
            UserPrivileges requiredPrivileges)
        {
            return MatchUserPrivilege(GetCurrentUserPrivileges(entity),
                requiredPrivileges);
        }

        protected bool IsCurrentUserNotSet()
        {
            return (CurrentUser == null);
        }

        public void SetRepository(IRepository repository)
        {
            Repository = repository;
        }

        #region Current User

        private FbCookieInfo m_currentUserFbInfo;

        protected FbCookieInfo CurrentUserFBInfo
        {
            get
            {
                if (m_currentUserFbInfo != null)
                {
                    return m_currentUserFbInfo;
                }

                var currentUserCookie = HttpContext.Request.Cookies["CurrentUserFBInfo"];
                var fbCookieInfo = (currentUserCookie == null) ?
                    new FbCookieInfo() : new FbCookieInfo(currentUserCookie.Values);

                fbCookieInfo.ValueUpdated +=
                    updatedFbCookieInfo => { CurrentUserFBInfo = updatedFbCookieInfo; };
                
                m_currentUserFbInfo = fbCookieInfo;
                return m_currentUserFbInfo;
            }

            set
            {
                if (value == null)
                {
                    Request.Cookies.Remove("CurrentUserFBInfo");
                    m_currentUserFbInfo = null;
                    Response.Cookies["CurrentUserFBInfo"].Expires = DateTime.Now.AddDays(-1);
                    return;
                }

                //TODO: encrypt cookie content
                var currentUserCookie = new System.Web.HttpCookie("CurrentUserFBInfo");
                /* "decrypt" facbook ID */

                currentUserCookie["FacebookId"] = value.FacebookId.ToString();
                currentUserCookie["AccessToken"] = value.AccessToken ;
                currentUserCookie["HasLikedPage"] = value.HasLikedPage.ToString();
                currentUserCookie["IsAdmin"] = value.IsAdmin.ToString();
                currentUserCookie["TimeOffset"] = value.TimeOffset.ToString();
                currentUserCookie["Signature"] = Convert.ToBase64String(value.Signature);
                currentUserCookie["IsCanvas"] = value.IsCanvas.ToString();

                if (Request.Cookies["CurrentUserFBInfo"] != null)
                {
                    Response.Cookies.Set(currentUserCookie);
                }
                else
                {
                    Response.Cookies.Add(currentUserCookie);
                }
            }
        }

        /// <summary>
        /// Get/set current session user
        /// </summary>
        ///
        /// <remarks>
        /// the session stores "encrypted" facebook ID, it should be converted
        /// into real user
        /// </remarks>
        protected FbUser CurrentUser
        {
            get
            {
                if (CurrentUserFBInfo == null)
                {
                    return null;
                }

                if (CurrentUserFBInfo.FacebookId ==0)
                {
                    return null;
                }

                long userId = CurrentUserFBInfo.FacebookId;
                var usr = Repository.Get<FbUser>(userId);
                return usr;
            }
        }
       
        #endregion

        protected string GetAppPageUrl(long fanPageId)
        {
            //TODO: Check if this works
            var appInfo = ConfigurationManager.GetSection("facebookSettings")
                as FacebookConfigurationSection;

            return string.Format(
                "https://www.facebook.com/pages/{0}/{0}?sk=app_{1}",
                fanPageId, appInfo.AppId);
        }

        protected UserPrivileges GetCurrentUserPrivileges(IEntity entity)
        {
            /* Check if values are set */
            if (IsCurrentUserNotSet() || entity == null)
            {
                return UserPrivileges.None;
            }

            if (CurrentUserFBInfo.IsAdmin)
            {
                return UserPrivileges.Owner;
            }

            /* Check if user owns is creator of entity */
            return entity.IsOwner(CurrentUser.Id) ?
                UserPrivileges.Owner : UserPrivileges.User;
        }

        #region DTOs

        /// <summary>
        /// create user DTO based on current user and additional information from
        /// specified playlist
        /// </summary>
        ///
        /// <param name="playlist">
        /// playlist to extract data from
        /// </param>
        ///
        /// <param name="defaultPriv">
        /// default user privileges to be set in the DTO, if not specified,
        /// will be detected based on current user access to specified playlist
        /// </param>
        ///
        /// <returns>
        /// mapped user DTO object
        /// </returns>
        protected UserDTO BuildUserDTO(Playlist playlist,
            UserPrivileges defaultPriv = UserPrivileges.NotSet)
        {
            var priv = (defaultPriv != UserPrivileges.NotSet) ?
                defaultPriv : GetCurrentUserPrivileges(playlist);

            var usrDTO = UserDTOFactory.Map(
                CurrentUser,
                CurrentUserFBInfo,
                playlist,
                priv);

            if (usrDTO != null)
            {
                return usrDTO;
            }

            /* return default values */
            return new UserDTO
                {
                    //these two values are always available via the connect cookie
                    IsPageAdmin = CurrentUserFBInfo.IsAdmin,
                    HasLikedPage = CurrentUserFBInfo.HasLikedPage,
                    TimeOffset = CurrentUserFBInfo.TimeOffset,
                    //default values
                    FBID = -1,
                    name = string.Empty,
                    numOfSongsLeft = -1,
                    role = "None",
                    score = 0,
                    summedScore = 0,
                };
        }

        /// <summary>
        /// build playlist DTO for specified local business
        /// </summary>
        ///
        /// <param name="lb">
        /// local business to convert to DTO
        /// </param>
        ///
        /// <param name="nvstr">
        /// translation strings
        /// </param>
        protected PageInfoDTO BuildPageInfoDto(LocalBusiness lb,
            IDictionary<string, string> nvstr)
        {
            var cust = Repository
                .Query<Customizations>()
                .FirstOrDefault(c => c.LocalBusiness == lb);
            var lbLocale = (cust != null) ? cust.Locale : null;
            /* assuming here that playlist owner is also localbusiness owner */
            var rv = PageInfoDTOFactory.Map(
                lb,
                BuildUserDTO(lb.DefaultPlaylist),
                HttpContext.Request.ApplicationPath,
                lbLocale);

            rv.IsCanvas = CurrentUserFBInfo.IsCanvas;
            rv.Strings = nvstr;
            return rv;
        }

        protected PlaylistWithUserDTO BuildPlaylistWithUserDTO(
            Playlist playlist)
        {
            return PlaylistWithUserDTOFactory.Map(playlist,
                BuildUserDTO(playlist));
        }

        #endregion

        protected JsonResult IsUserAllowedToEditEntity(IEntity entity)
        {
            if (!MatchUserPrivilege(entity, UserPrivileges.Owner))
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }
            return null;
        }

        #region User Info

        protected void RewardUser(Playlist playlist,
            UserPlaylistInfo.Operation operation)
        {
            var currentUserInfo = GetUserInfo(playlist, CurrentUser);
            if (currentUserInfo == null)
            {
                return;
            }

            currentUserInfo.UpdateUserScore(operation);
            Repository.SaveOrUpdate(currentUserInfo);
        }

        private static UserPlaylistInfo GetUserInfo(
            Playlist playlist, FbUser user)
        {
            return (playlist == null) ? null : playlist.GetUserInfo(user);
        }

        #endregion

        /// <summary>
        /// build failure result to be returned to client
        /// </summary>
        ///
        /// <param name="success">
        /// returned status of operation
        /// </param>
        ///
        /// <param name="error">
        /// error message to be attached to the status message
        /// </param>
        ///
        /// <returns>
        /// resulting JSON object to be returned to the caller
        /// </returns>
        protected JsonResult BuildFailureResult(short success, string error)
        {
            return BuildResult(success, error, string.Empty);
        }

        /// <summary>
        /// build success result to be returned to client
        /// </summary>
        ///
        /// <param name="success">
        /// returned status of operation
        /// </param>
        ///
        /// <param name="data">
        /// data object to attach to the response
        /// </param>
        ///
        /// <returns>
        /// rendered data object
        /// </returns>
        protected JsonResult BuildSuccessResult(short success, object data)
        {
            return BuildResult(success, string.Empty, data);
        }

        /// <summary>
        /// build general result in JSON format
        /// </summary>
        ///
        /// <param name="success">
        /// status of operation
        /// </param>
        ///
        /// <param name="error">
        /// error message to be attched to the message
        /// </param>
        /// 
        /// <param name="data">
        /// data object of the message
        /// </param>
        ///
        /// <returns></returns>
        protected JsonResult BuildResult(short success, string error,
            object data)
        {
            return Json(new ResponseStatus
            {
                Status = success,
                Error = error,
                Data = data
            });
        }

        /// <summary>
        /// load all the string from teh resource
        /// </summary>
        ///
        /// <returns>
        /// strings loaded from resource file
        /// </returns>
        protected static IDictionary<string, string> LoadStrings()
        {
            var allStrings =
                MResourcesNames.Select(LoadStrings).ToList();
            return allStrings.SelectMany(dict => dict)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// load specific resource from current assembly
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        private static IDictionary<string, string> LoadStrings(string ns)
        {
            var asm = Assembly.GetExecutingAssembly();
            var rm = new ResourceManager(ns, asm);
            var ci = Thread.CurrentThread.CurrentCulture;
            var res = rm.GetResourceSet(ci, true, true);
            var en = res.GetEnumerator();
            var pairs = new Dictionary<string, string>();
            while (en.MoveNext())
            {
                pairs.Add(en.Key.ToString(), en.Value.ToString());
            }
            return pairs;
        }

        /// <summary>
        /// response rendered by every call
        /// </summary>
        public class ResponseStatus
        {
            public short Status { get; set; }
            public string Error { get; set; }
            public object Data { get; set; }
        }

        /// <summary>
        /// error message model for error page
        /// </summary>
        public class ErrorMessage
        {
            /// <summary>
            /// default constructor of the object
            /// </summary>
            ///
            /// <param name="details">
            /// details of the error message
            /// </param>
            public ErrorMessage(string details)
            {
                Details = details;
            }

            /// <summary>
            /// get the title of the error message
            /// </summary>
            public string Title
            {
                get { return App_GlobalResources.Strings.defaultError; }
            }

            /// <summary>
            /// get the details of the error message
            /// </summary>
            public string Details { get; private set;}
        }
    }
}
