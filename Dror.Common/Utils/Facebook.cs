using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Dror.Common.Utils.Contracts;
using Facebook;
using System.Collections.Generic;

namespace Dror.Common.Utils
{
    public class Facebook
    {
        private delegate object GraphCommand(string command);

        /// <summary>
        /// JSON object representation as received from Facebook
        /// </summary>
        [DataContract]
        private class LocalFbUser
        {
            [DataMember(Name = "id")]
            internal long Id = default(long);

            [DataMember(Name = "name")]
            internal string Name = default(string);

            [DataMember(Name = "first_name")]
            internal string FirstName = default(string);

            [DataMember(Name = "last_name")]
            internal string LastName = default(string);

            [DataMember(Name = "link")]
            internal string Link = default(string);

            [DataMember(Name = "gender")]
            internal string Gender = default(string);

            [DataMember(Name = "locale")]
            internal string Locale = default(string);
        }

        /// <summary>
        /// URL of graph interface
        /// </summary>
        private const string MFbGraphApi = @"https://graph.facebook.com/";

        /// <summary>
        /// types of feed posts
        /// </summary>
        public enum WallPostType
        {
            /// <summary>
            /// regular wall post
            /// </summary>
            post,

            /// <summary>
            /// video posted to the wall
            /// </summary>
            video,

            /// <summary>
            /// photo posted to the wall
            /// </summary>
            photo
        }

        /// <summary>
        /// Kululu application fan page ID
        /// </summary>
        public const long APP_FAN_PAGE_ID = 178456328876503;

        /// <summary>
        /// retrieve single page from facebook graph API
        /// </summary>
        ///
        /// <param name="fbid">
        /// Facebook ID of the object to retrieve
        /// </param>
        ///
        /// <returns>
        /// retrieved page or null on failure
        /// </returns>
        public static string FacebookGetPage(string fbid)
        {
            using (var wc = new WebClient())
            {
                var graphUrl = String.Format("{0}{1}", MFbGraphApi, fbid);
                return wc.DownloadString(graphUrl);
            }
        }

        /// <summary>
        /// retrieve the user information from the graph interface of Facebook
        /// and extract full name of the user.
        /// </summary>
        ///
        /// <param name="fbid">
        /// Facebook ID of the user
        /// </param>
        ///
        /// <returns>
        /// full name of the user or null on failure
        /// </returns>
        public static string GetFbUserFullName(long fbid)
        {
            var resp = FacebookGetPage(fbid.ToString());
            if (String.IsNullOrEmpty(resp) || resp.Equals("false"))
            {
                return null;
            }

            var ser = new DataContractJsonSerializer(typeof(LocalFbUser));
            var usr = (LocalFbUser)ser.ReadObject(new MemoryStream(
                Encoding.Unicode.GetBytes(resp)));
            return (usr == null) ? null : usr.Name;
        }

        public static string PostVideoPlayerToWall(FacebookClient fbapp,
            long fanPageId, 
            string link,
            string title,
            string msg,
            string description,
            Dictionary<string, string> properties)
        {
            dynamic postData = new ExpandoObject();
            postData.link = link;
            postData.source = null;
            postData.picture = null;
            postData.message = msg;
            postData.caption = "www.youtube.com";
            postData.description = description;
            postData.name = title;

            var postPath = string.Format("{0}/feed", fanPageId);
            dynamic response = fbapp.Post(postPath, postData);
            return response.id as string;
        }

        /// <summary>
        /// post a single post to wall
        /// </summary>
        ///
        /// <param name="fbapp">
        /// Facebook app used to post to wall
        /// </param>
        ///
        /// <param name="fanPageId">
        /// fan page ID to post to the wall
        /// </param>
        ///
        /// <param name="title">
        /// title of the post
        /// </param>
        ///
        /// <param name="msg">
        /// message to be posted to the wall
        /// </param>
        ///
        /// <param name="imgUrl">
        /// URL of the image to attach to this post
        ///  </param>
        ///
        /// <param name="link">
        /// link attached to this post
        /// </param>
        ///
        ///<returns>
        /// true if the post succeeded, false otherwise
        /// </returns>
        public static string PostToWall(FacebookClient fbapp,
            long fanPageId,
            string title,
            string msg,
            string imgUrl,
            string link,
            string source,
            string description,
            string type,
            Dictionary<string, string> properties)
        {
            /* input sanity check */
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(msg))
            {
                return string.Empty;
            }
            dynamic postData = new ExpandoObject();

            ///* ensure, there are no "nulls" in the message */
            if (msg == null)
            {
                msg = string.Empty;
            }
            if (title == null)
            {
                title = string.Empty;
            }

            postData.message = msg;
            postData.link = link;
            postData.source = source;

            postData.caption = "www.youtube.com";
            postData.name = title;
            postData.picture = imgUrl;
            postData.description = description;
            postData.type = type;

            if (properties != null)
            {
                postData.properties = properties;
            }
            
            var postPath = string.Format("{0}/feed", fanPageId);
            dynamic response = fbapp.Post(postPath, postData);
            return response.id as string;
        }

        /// <summary>
        /// find access token of the relevant page
        /// </summary>
        ///
        /// <param name="fbapp">
        /// Facebook application to be used to extract data
        /// </param>
        ///
        /// <param name="fanPageId">
        /// ID of the fan page to post to
        /// </param>
        ///
        /// <param name="ownerId">
        /// owner of the fan page who has given permission to the application
        /// to access his data offline
        /// </param>
        ///
        /// <returns>
        /// page access token or null on failure
        /// </returns>
        public static string GetPageAccessToken(
            FacebookClient fbapp,
            long fanPageId,
            long ownerId)
        {
            /* parsed page ID */
            long fpid;

            /* get pages of current user */
            dynamic pagesInfo =
                fbapp.Get(String.Format("/{0}/accounts", ownerId));
            if (pagesInfo == null || pagesInfo.data == null)
            {
                return null;
            }

            foreach (dynamic pg in pagesInfo.data)
            {
                if (pg == null || pg.id == null)
                {
                    continue;
                }

                var res = long.TryParse(pg.id, out fpid);
                if (!res)
                {
                    continue;
                }

                if (fpid == fanPageId)
                {
                    return pg.access_token;
                }
            }

            return null;
        }


        public static bool SetFbLike(string fBPostId, short rating, FacebookClient fbapp)
        {
            GraphCommand likeCall = null;
            if (rating <= 0)
            {
                likeCall = new GraphCommand(
                 (likeCommand) => fbapp.Delete(likeCommand, null)
                );
            }

            else if (rating > 0)
            {
                likeCall = new GraphCommand(
                 (likeCommand) => fbapp.Post(likeCommand, null)
                );
            }
            return RunLikeGraphCommand(fBPostId, likeCall);
        }


        private static bool RunLikeGraphCommand(string postId, GraphCommand graphCmd)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return false;
            }

            string likeCommand = string.Format("{0}/likes", postId);
            var response = graphCmd(likeCommand);

            bool isSuccess;
            if (bool.TryParse(response.ToString(), out isSuccess))
            {
                return isSuccess;
            }
            return false;
        }

        public static bool RemovePost(string fBPostId, FacebookClient fbapp)
        {
            string graphCmd = string.Format("/{0}", fBPostId);
            var response = fbapp.Delete(graphCmd);
            bool success;
            if (bool.TryParse(response.ToString(), out success))
            {
                return success;
            }
            return false;
        }

        /// <summary>
        /// check whether all the specified users are admins of the page
        /// </summary>
        ///
        /// <param name="fbapp">
        /// Facebook application to use to retrieve data
        /// </param>
        ///
        /// <param name="pageId">
        /// ID of the page to be tested
        /// </param>
        ///
        /// <param name="userIds">
        /// IDs of all the users to be tested
        /// </param>
        ///
        /// <param name="log">
        /// logger to be used for error reporting
        /// </param>
        ///
        /// <returns>
        /// dictionary of all users specified as an input with values of admin
        /// flag
        /// </returns>
        public static Dictionary<string, bool> AreAdmins(
            FacebookClient fbapp,
            long pageId,
            string[] userIds,
            ILogger log)
        {

            var index = 0;
            var requests = new object[userIds.Length];
            foreach (var userId in userIds)
            {
                var url = string.Format("{0}/admins/{1}", pageId, userId);

                var singleRequest = new
                {
                    method = "GET",
                    relative_url = url
                };

                requests[index] = singleRequest;
                index++;
            }

            var batchRequest = new Dictionary<string, object> { { "batch", requests } };

            index = 0;
            var admins = new Dictionary<string, bool>();
            try
            {
                dynamic rslts = fbapp.Post("https://graph.facebook.com/",
                    batchRequest);
                foreach (var rslt in rslts)
                {
                    // does the result contain the name of the admin
                    var isAdmin = rslt.body.Contains("name");
                    admins.Add(userIds[index], isAdmin);
                    index++;
                }
            }
            catch (FacebookApiException e)
            {
                if (log != null)
                {
                    log.Warn("Failed to retrieve admins", e);
                }
            }
            return admins;
        }

        public static string GetLinkToApp(string pageName, long fanPageId, string appId, string appData = null)
        {
            return string.Format("https://www.facebook.com/pages/{0}/{1}?sk=app_{2}&app_data={3}",
                           pageName,
                           fanPageId, appId, appData);
        }
    }
}
