using System;
using System.Linq;
using System.Text.RegularExpressions;
using Facebook;
using Kululu.Entities;
using Kululu.Entities.Common;
using Dror.Common.Data.Contracts;
using Dror.Common.Data.NHibernate;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Dror.Common.Utils.Contracts;
using System.Diagnostics;
using System.Threading;

namespace Kululu.FBStreamHarvest
{
    /// <summary>
    /// reader of facebook social stream
    /// </summary>
    public class FacebookStreamReader : ISocialStreamReader
    {
        #region Status

        public event FbStreamUpdate FbStreamUpdated;

        public IList<StreamUpdateStatus> Status { get; set; }

        #endregion

        #region Consts
        /// <summary>
        /// separator between the artist name in song title in YouTube file
        /// title
        /// </summary>
        private static readonly char[] SONG_TITLE_SEPARATORS = new[] { '-' };

        private const int WAIT_BETWEEN_OPERATIONS = 2000;
        /// <summary>
        /// Parser of video ID from YouTube links
        /// </summary>
        private static readonly Regex MYoutubeRex = new Regex(
            @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");

        /// <summary>
        /// Number of different queries that can be sent in a single requests
        /// to facebook
        /// </summary>
        private const int FB_MULTIQUERY_LIMIT = 1;

        /// <summary>
        /// number of records to retreive from facebook's server in a single
        /// request
        /// </summary>
        private const int FB_FETCH_LIMIT = 200;

        /// <summary>
        /// for small queries, that return few records at a time, we use this
        /// limit because the length of the querystring itself can get so big
        /// that it's no longer legal
        /// </summary>
        private const int SMALL_FQL_QUERY_LENGTH_LIMIT = 250;

        /// <summary>
        /// Max num of records facebook is willing to return for a single query
        /// </summary>
        private const int SINGLE_FQL_QUERY_LIMIT = 4500; // real limit is 5000, we're taking a 

        /// <summary>
        /// URL of the graph API request to retrieve the feed data
        /// </summary>
        private const string M_GET_ALL_POSTS_REQUEST_URL = "/{0}/posts";

        private const string M_GET_LAST_WEEK_FEEDS_REQUEST_URL =
            "/{0}/posts?since=Last Week";

        private const string M_GET_LAST_DAY_FEEDS_REQUEST_URL =
            "/{0}/posts?since=Yesterday";

        /// <summary>
        /// URL of the graph API request to retrieve user data
        /// </summary>
        private const string M_USER_REQUEST_URL = "{0}";

        #endregion

        #region Properties
        public int NumOfLikes { get; set; }

        public int LikesRetrieved { get; set; }

        private string SavedFbInfoPath { get; set; }

        /// <summary>
        /// Facebook app instance
        /// </summary>
        FacebookClient FbApp { get; set; }

        /// <summary>
        /// dictionary used to convert local timespan into relevant Facebook
        /// URL
        /// </summary>
        private readonly static Dictionary<HarvestType, string> MTsDict =
            new Dictionary<HarvestType, string>
                {
                    {HarvestType.Last24hrs, M_GET_LAST_DAY_FEEDS_REQUEST_URL},
                    {HarvestType.LastWeek, M_GET_LAST_WEEK_FEEDS_REQUEST_URL},
                    {HarvestType.All, M_GET_ALL_POSTS_REQUEST_URL},
                    {HarvestType.TopTen, M_GET_ALL_POSTS_REQUEST_URL}
                };

        private long UniqueStatusIndentifier { get; set; }

        private Action<long, int, int> LikesRetrivalNotificationHandler
        {
            get;
            set;
        }

        private Action<long, StreamUpdateStatus> StatusNotificationHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Repository used to talk to the database
        /// </summary>
        private IRepository Repository { get; set; }

        private ILogger Logger { get; set; }

        #endregion
        
        /// <summary>
        /// default constructor of the object
        /// </summary>
        public FacebookStreamReader(
            Action<long, StreamUpdateStatus> statusNotificationHandler,
            Action<long, int, int> likesRetrivalNotificationHandler,
            long uniqueStatusIndentifier,
            string fbFilePath,
            ILogger logger,
            FacebookClient fbApp,
            IRepository repository)
        {
            Logger = logger;
            Logger.Info("Started");

            UniqueStatusIndentifier = uniqueStatusIndentifier;
            StatusNotificationHandler = statusNotificationHandler;
            LikesRetrivalNotificationHandler =
                likesRetrivalNotificationHandler;

            Status = new List<StreamUpdateStatus>();
            FbStreamUpdated += FacebookStreamReaderFbStreamUpdated;

            /* settings of the application */
            SavedFbInfoPath = fbFilePath;

            /* facebook application interface */
            FbApp = fbApp;
            Repository = repository;
            Repository.BeginTransaction();
        }

        void FacebookStreamReaderFbStreamUpdated(
            StreamUpdateStatus status,
            Exception ex = null)
        {
            if (status != StreamUpdateStatus.Error)
            {
                Logger.InfoFormat("Just entered {0} stage", status);
            }
            else if (ex != null)
            {
                Logger.Error("An error has occured", ex);
            }
            else
            {
                Logger.Error("An error has occured");
            }

            StatusNotificationHandler(UniqueStatusIndentifier, status);
            Status.Add(status);
        }

        /// <summary>
        /// convert local format of timespan into date/time value
        /// </summary>
        ///
        /// <param name="type">
        /// local timespan value
        /// </param>
        ///
        /// <returns>
        /// Real time span
        /// </returns>
        private static DateTime? ConvertTimespan(HarvestType type)
        {
            switch (type)
            {
                case HarvestType.Last24hrs:
                    return DateTime.Now.AddDays(-1);
                case HarvestType.LastWeek:
                    return DateTime.Now.AddDays(-7);
                case HarvestType.TopTen:
                case HarvestType.All:
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// start synchronization of specified localbusiness stream
        /// </summary>
        public void Synchronize(long localBusinessId, HarvestType type)
        {
            try
            {
                var timeSpan = ConvertTimespan(type);

                // we really want all playlists
                var ratingsList = Repository
                    .GetOrderedGrandChildren<PlaylistSongRating>(
                        typeof(PlaylistSongRating),
                        "Playlist",
                        "LocalBusiness",
                        localBusinessId,
                        "FacebookAddedDate",
                        0,
                        int.MaxValue,
                        timeSpan)
                    .Where(rating => !string.IsNullOrEmpty(rating.FBPostId))
                    .ToList();

                if (ratingsList.Count > 0)
                {
                    Synchronize(ratingsList,
                        ratingsList[0].Playlist.LocalBusiness);
                }
                FbStreamUpdated(StreamUpdateStatus.Finished);
            }
            catch(Exception ex)
            {
                Logger.Error("The following error has occured", ex);
            }
        }

        public void Synchronize()
        {
            var lbs = Repository.Query<LocalBusiness>();
            foreach (var lb in lbs)
            {
                // we really want all playlists
                var ratingsList = Repository
                    .GetOrderedGrandChildren<PlaylistSongRating>(
                        typeof(PlaylistSongRating),
                        "Playlist",
                        "LocalBusiness",
                        lb.Id,
                        "FacebookAddedDate",
                        0,
                        int.MaxValue);
                Synchronize(ratingsList.ToList(), lb);
            }
            FbStreamUpdated(StreamUpdateStatus.Finished);
        }

        /// <summary>
        /// Converting from PlaylistSongRating to PlaylistSongRatingAndLikes
        /// and continuing synchronization
        /// </summary>
        private void Synchronize(IEnumerable<PlaylistSongRating> fbPosts,
            LocalBusiness lb)
        {
            var postInfoWithLikes = new List<PlaylistSongRatingAndLikes>();
            foreach (var postInfo in fbPosts)
            {
                postInfoWithLikes.Add(new PlaylistSongRatingAndLikes
                {
                    PlaylistSongRating = postInfo,
                    NumOfLikes = 0 //setting default values
                });
            }

            Synchronize(postInfoWithLikes, lb);
        }

        private void Synchronize(List<PlaylistSongRatingAndLikes> postsInfo,
            LocalBusiness lb)
        {
            if (postsInfo.Count == 0)
            {
                return;
            }
            
            FbStreamUpdated(StreamUpdateStatus.RetrievingAggregatedInfo);

            Logger.InfoFormat("entering GetAggregatedInfoFqlRequests");
            var requestsQuery = GetAggregatedInfoFqlRequests(postsInfo);
            Logger.InfoFormat("exiting GetAggregatedInfoFqlRequests");
            

            Logger.InfoFormat("about to request {0} queries from facebook", requestsQuery.Count);

            var tasks = new List<Task>();
            var taskFactory =
                    new TaskFactory(
                        TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness,
                       TaskContinuationOptions.LongRunning | TaskContinuationOptions.PreferFairness);

            for (var index = 0; index < requestsQuery.Count; index++)
            {
                dynamic response = null;
                try
                {
                    Logger.InfoFormat("requesting {0} / {1}",
                    index + 1, requestsQuery.Count);
                    response = FbApp.Query(requestsQuery[index]);
                    Logger.InfoFormat("recieved aggregated info from facebook");
                }
                catch (Exception ex)
                {
                    Logger.Fatal("The following error has occured", ex);
                }

                var task = taskFactory.StartNew(() =>
                {
                    Logger.InfoFormat("entering SaveAggregatedData of reqest number {0}", index);
                    SaveAggregatedData(postsInfo, response);
                    Logger.InfoFormat("left SaveAggregatedData of reqest number {0}", index);
                });

                tasks.Add(task);
            }
            
            // waiting for last thread to compelte execution before exiting
            if (tasks.Count > 0)
            {
                Logger.Info("Waiting for all likes to be retrieived");
                Task.WaitAll(tasks.ToArray());
            }

            Logger.Info("commiting aggregated info");
            Repository.CommitTransaction();
            Logger.Info("complete commit of aggregated info");
            
            //Thread.Sleep(WAIT_BETWEEN_OPERATIONS);
            SaveLikesInfo(postsInfo, lb);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SaveAggregatedData(
            List<PlaylistSongRatingAndLikes> fbPosts, dynamic fbInfo)
        {
            try
            {
                //TODO: Really... refactor this!
                foreach (var info in fbInfo)
                {
                    var posts = fbPosts.Where(post => post.PlaylistSongRating.FBPostId == info["post_id"]);
                    foreach (var post in posts)
                    {
                        var numOfComments =
                            int.Parse(info["comments"]["count"]);

                        var numOfLikes = int.Parse(info["likes"]["count"]);

                        post.NumOfLikes = numOfLikes;

                        post.PlaylistSongRating.NumOfComments =
                            numOfComments;

                        Repository.SaveOrUpdate(post.PlaylistSongRating);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("The following error has occured", ex);
            }
        }

        /// <summary>
        /// import single local business data
        /// </summary>
        ///
        /// <param name="lbid">
        /// local business ID
        /// </param>
        ///
        /// <param name="type">
        /// type of the import: required timespan actually
        /// </param>
        void ISocialStreamReader.Import(long lbid, HarvestType type)
        {
            var lb = Repository.Get<LocalBusiness>(lbid);

            Logger.Info(string.Format("Starting import for business {0}",
                lb.Name));

            var lbs = new[] { lb };
            if (MTsDict.ContainsKey(type))
            {
                Import(lbs, MTsDict[type], type);
            }
            else
            {
                Logger.ErrorFormat("Invalid time span: {0}", type);
            }
            FbStreamUpdated(StreamUpdateStatus.Finished);
        }

        /// <summary>
        /// start synchronous stream reading
        /// </summary>
        void ISocialStreamReader.Import()
        {
            Logger.Info("Starting Import");
            var lbs = Repository.Query<LocalBusiness>();
            Import(lbs, M_GET_ALL_POSTS_REQUEST_URL, HarvestType.NoneSpecified);
            FbStreamUpdated(StreamUpdateStatus.Finished);
        }

        /// <summary>
        /// import posts of all the specified fan pages with specified query
        /// </summary>
        ///
        /// <param name="lbsE">
        /// fan pages to be processed
        /// </param>
        ///
        /// <param name="graphQuery">
        /// graph API query to use for retrieval of posts
        /// </param>
        private void Import(IEnumerable<LocalBusiness> lbsE, string graphQuery, HarvestType type)
        {
            /* input sanity check */
            if (lbsE == null)
            {
                throw new ArgumentNullException("lbsE");
            }

            /* convert IEnumerable to list */
            var lbs = lbsE.ToList();

            try
            {
                FbStreamUpdated(StreamUpdateStatus.Intiailized);

                if (lbs.Count() == 0)
                {
                    FbStreamUpdated(StreamUpdateStatus.Error);
                    return;
                }

                Repository.BeginTransaction();
                var tasks = new List<Task>();
                var taskFactory = new TaskFactory(
                            TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness,
                            TaskContinuationOptions.LongRunning | TaskContinuationOptions.PreferFairness);

                foreach (var fp in lbs)
                {
                    FbStreamUpdated(StreamUpdateStatus.RetrievingPosts);

                    var postsInfos = ProcessFanPage(fp, graphQuery, type);

                    // commiting changes 
                    Repository.CommitTransaction();

                    FbStreamUpdated(StreamUpdateStatus.RetrievingLikes);

                    if (postsInfos != null)
                    {
                        var task = taskFactory.StartNew(
                            l => SaveLikesInfo(postsInfos, (LocalBusiness)l),
                            fp);

                        tasks.Add(task);
                    }
                }

                // waiting for last thread to compelte execution before exiting
                if (tasks.Count > 0)
                {
                    Logger.Info("Waiting for all likes to be retrieived");
                    Task.WaitAll(tasks.ToArray());
                }

                Repository.CommitTransaction();
            }
            catch (Exception ex)
            {
                FbStreamUpdated(StreamUpdateStatus.Error, ex);
            }
        }

        /// <summary>
        /// process wall messages of a single fan page
        /// </summary>
        ///
        /// <param name="fp">
        /// fan page to process
        /// </param>
        ///
        /// <param name="graphQuery">
        /// graph API query to use when processing this fan page
        /// </param>
        private List<PlaylistSongRatingAndLikes> ProcessFanPage(
            LocalBusiness fp, string graphQuery, HarvestType type)
        {
            Logger.InfoFormat("Processing fan page {0}: {1}... ",
                fp.FanPageId, fp.Name);

            /* build the initial URL of the page */
            var feedUrl = string.Format(graphQuery, fp.FanPageId);

            var defaultUser = GetDefaultUser(fp, graphQuery);
            var postsInfo = new List<PlaylistSongRatingAndLikes>();

            var isReadComplete = false;
            dynamic parameters = new ExpandoObject();
            parameters.limit = FB_FETCH_LIMIT;
            parameters.offset = 0;

            var data = FbApp.Get(feedUrl, parameters);
            do
            {
                Logger.Info("fetching posts");
                if (!data.ContainsKey("data"))
                {
                    return postsInfo;
                }

                foreach (var dat in data["data"])
                {
                    try
                    {
                        var postLink = GetVideoLink(dat);
                        var postType = dat.ContainsKey("type") ?
                            (string)dat["type"] : string.Empty;

                        /* in order to simplify conditions, continue is used
                         * otherwise, "if's" are too comples */
                        if (postType == null || postLink == null)
                        {
                            continue;
                        }

                        /* filter allowed content */
                        if (!postType.Equals("video") &&
                            !postType.Equals("swf") &&
                            !postType.Equals("link"))
                        {
                            continue;
                        }

                        /* process only YouTube links */
                        if (!postLink.Contains("youtube.com"))
                        {
                            continue;
                        }

                        var postInfo = ProcessLink(fp, dat, defaultUser);

                        /* wasn't previously added, handling cases of duplicate
                         * posts */
                        if (postInfo != null && !postsInfo.Contains(postInfo))
                        {
                            postsInfo.Add(postInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        FbStreamUpdated(StreamUpdateStatus.Error, ex);
                        Logger.Error("This should not have happened", ex);
                        return null;
                    }

                    if (type == HarvestType.TopTen && postsInfo.Count >= 10)
                        break;
                }

                if (type == HarvestType.TopTen && postsInfo.Count >= 10)
                    break;

                Logger.InfoFormat("finished fetching {0} posts",
                    postsInfo.Count);

                if (!data.ContainsKey("paging") ||
                    !data["paging"].ContainsKey("next"))
                {
                    isReadComplete = true;
                }
                else
                {
                    var serializer = new JavaScriptSerializer();
                    string nextPage = data["paging"]["next"];

                    if (graphQuery.ToLower().Contains("since"))
                    {
                        nextPage =
                            string.Format("{0}&since=yesterday", nextPage);
                    }

                    var request = new MyWebRequest(nextPage);
                    data = serializer.Deserialize<dynamic>(
                        request.GetResponse());
                }
            } while (!isReadComplete);

            Logger.InfoFormat("Done processing fan page {0}: {1}",
                fp.FanPageId, fp.Name);

            return postsInfo;
        }

        /// <summary>
        /// When querying posts we find that the default user who added the
        /// songs is the page itself. Since we don't want to save the page as a
        /// user, we specify another default user to attach the songs to
        /// </summary>
        private static FbUser GetDefaultUser(LocalBusiness lb,
            string graphRequest)
        {
            return graphRequest.ToLower().Contains("posts") ?
                lb.Owners.FirstOrDefault() : null;
        }

        /// <summary>
        /// should the data of this local business be imported?
        /// </summary>
        private static bool ShouldImport(LocalBusiness lb)
        {
            return (lb.ImportPlaylist != null);
        }

        /// <summary>
        /// process single wall post with link
        /// </summary>
        ///
        /// <param name="fp">
        /// fan page this post belongs to
        /// </param>
        ///
        /// <param name="post">
        /// facebook post as recevied from facebook API
        /// </param>
        ///
        /// <param name="user">
        /// user posting this link
        /// </param>
        ///
        ///<returns></returns>
        private PlaylistSongRatingAndLikes ProcessLink(
            LocalBusiness fp,
            dynamic post,
            FbUser user = null)
        {
            var name = post.ContainsKey("name") ?
                (string)post["name"] : string.Empty;

            var caption = post.ContainsKey("caption") ?
                (string)post["caption"] : string.Empty;

            var id = post.ContainsKey("id") ?
                (string)post["id"] : string.Empty;

            // TODO: move this to some method
            var link = GetVideoLink(post);

            var addedDate = Convert.ToDateTime(post["created_time"]);

            var from = post["from"];
            var fromId = from.ContainsKey("id") ? from["id"] : 0;

            var numOfComments = (int)post["comments"]["count"];

            var numOfLikes = post.ContainsKey("likes") ?
                (int)post["likes"]["count"] : 0;
            var message = post.ContainsKey("message") ?
                post["message"] : null;

            var description = post.ContainsKey("description") ?
                post["description"] : null;

            /* if not record was found and localbusiness doesn't allow import
             * of new content, exit */
            if (!ShouldImport(fp))
            {
                return null;
            }

            /* try to find this song in the database or create one */
            if (user == null)
            {
                user = FindOrCreateUser(fromId);
            }

            var song = FindOrCreateSong(link, name, caption) as Song;
            if (song == null)
            {
                return null;
            }

            // song doesn't exist in DB yet
            if (song.Id == 0)
            {
                Repository.Save(song);
            }

            var rating = Repository.Query<PlaylistSongRating>()
                .Where(r => r.Playlist == fp.ImportPlaylist)
                .Where(r => r.FBPostId == id)
                .FirstOrDefault();

            /* if this rating is already stored in the database, do nothing */
            if (rating == null)
            {
                rating = fp.ImportPlaylist.AddSong(song, user, true, 0);
                rating.FBMessage = message;
                rating.FBDescription = description;
                rating.Origin = Origin.Facebook;
                rating.FBPostId = id;
                rating.FacebookAddedDate = addedDate;
            }
            else
            {
                Logger.InfoFormat(
                    "The song with the {0} FBPostId already exists under this FbPostId {1}",
                    id,
                    rating.FBPostId);
            }

            rating.NumOfComments = numOfComments;
            Repository.SaveOrUpdate(rating);
            Repository.SaveOrUpdate(fp.ImportPlaylist);

            return new PlaylistSongRatingAndLikes
                {
                    PlaylistSongRating = rating,
                    NumOfLikes = numOfLikes
                };
        }

        private static string GetVideoLink(dynamic post)
        {
            string youtubeUrl = string.Empty;

            if (post.ContainsKey("link"))
            {
                youtubeUrl = (string) post["link"];
            }

            if (!youtubeUrl.ToLower().Contains("youtube"))
            {
                if (post.ContainsKey("source"))
                {
                    youtubeUrl = (string) post["source"];
                }
            }

            if (!youtubeUrl.ToLower().Contains("youtube"))
            {
                if (post.ContainsKey("message"))
                {
                    youtubeUrl = (string)post["message"];
                }
            }

            return youtubeUrl;
        }

        private void SaveLikesInfo(
            IEnumerable<PlaylistSongRatingAndLikes> postsInfo,
            LocalBusiness lb)
        {
            FbStreamUpdated(StreamUpdateStatus.RetrievingLikes);

            Logger.InfoFormat("entering GetLikesFqlRequests");
            var requestsQuery = GetLikesFqlRequests(postsInfo);
            Logger.InfoFormat("exiting GetLikesFqlRequests");

            Logger.InfoFormat("Complete num of likes to synchronize is {0}", NumOfLikes);

            var increment = 0;

            Logger.InfoFormat("requesting {0} queries", requestsQuery.Count);
            var myProcess = Process.GetCurrentProcess();

            while (increment < requestsQuery.Count)
            {
                try
                {
                    Logger.InfoFormat("memory footprint : {0} bytes", myProcess.WorkingSet64);

                    var queries = requestsQuery
                        .Skip(increment)
                        .Take(FB_MULTIQUERY_LIMIT)
                        .ToArray();

                    increment += FB_MULTIQUERY_LIMIT;

                    Logger.InfoFormat("requesting facebook like batch number {0}",
                            increment);

                    dynamic response = FbApp.Query(queries);
                    Logger.Info("retrieved likes info");
                    string sqlInsertInto = CreateSqlInsertInto(response, lb);
                    PersistToDB(sqlInsertInto);
                }
                catch (Exception ex)
                {
                    var errMsg = string.Format(
                        "Couldn't retrieve all info from facebook. batch number {0} / {1}",
                        increment,
                        requestsQuery.Count);
                    Logger.Error(errMsg, ex);
                }
                finally
                {
                    Logger.InfoFormat(
                        "batch number {0} / {1}",
                        increment,
                        requestsQuery.Count);
                }
            }

            FbStreamUpdated(StreamUpdateStatus.RetrievingLikesComplete);
        }

        /// <summary>
        /// 
        /// </summary>
        ///
        /// <param name="fbLikeData">
        /// </param>
        ///
        /// <remarks> 
        /// This method is Synchronized, not two calls can be made at the same
        /// time.
        /// Calls will be stacked.
        /// </remarks>
        private string CreateSqlInsertInto(dynamic fbLikeData, LocalBusiness lb)
        {
            var fanPageId = lb.FanPageId.ToString();

            var numOfLikes = 0;
            var insertIntoBuilder = new StringBuilder(3145728);
            insertIntoBuilder.Append(" INSERT INTO {0} (isAdmin ,postId , userId , objectId ) values");

            foreach (var queryRslt in fbLikeData)
            {
                foreach (var rsltDetails in queryRslt.fql_result_set)
                {
                    // appending is admin value
                    insertIntoBuilder.AppendFormat(
                         "  ({0}, '{1}', {2}, {3}), ",
                        rsltDetails.user_id == fanPageId ? 1 : 0,
                        rsltDetails.post_id,
                        rsltDetails.user_id,
                        rsltDetails.object_id
                        );
                    numOfLikes++;
                }
            }

            insertIntoBuilder.Length -= 2;
            insertIntoBuilder.Append(";");

            AddNumOfLikesAdded(numOfLikes);
            return insertIntoBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void PersistToDB(string insertInto)
        {
            try
            {
                Logger.Info("Running sql...");
                RunSqlUpdates(insertInto);
                Logger.Info("Complete running Sql");
            }
            catch (Exception ex)
            {
                Logger.Error("The following error has occured", ex);
            }

            Thread.Sleep(WAIT_BETWEEN_OPERATIONS);
            Logger.InfoFormat("SaveData complete");
        }

        private void RunSqlUpdates(string insertInto)
        {
            if (string.IsNullOrEmpty(insertInto))
            {
                return;
            }

            /* generate temporary table name */
            var tempFBWarehouseName = string.Format(
                "facebook_warehouse_{0}",
                Guid.NewGuid().ToString().Replace("-","_"));

            insertInto = string.Format(insertInto, tempFBWarehouseName);

            var tblBuilder = new StringBuilder();

            //http://jeffrick.com/2010/03/23/bulk-insert-into-a-mysql-database/
            /* Drop TEMPORARY TABLE kululu_radio.my_temp_table */

            //Creating temporary able
            tblBuilder.AppendLine(" START TRANSACTION; ");
            tblBuilder.AppendFormat(" CREATE /*TEMPORARY*/ TABLE IF NOT EXISTS {0} (isAdmin TinyInt(1) ,postId char(100), userId char(100), objectId char(100)); ", tempFBWarehouseName);
            tblBuilder.AppendFormat(" TRUNCATE TABLE {0}; ", tempFBWarehouseName);
            tblBuilder.AppendLine(insertInto);

            var tempFBUserVotes = string.Format("fbUserVotes_{0}",
                Guid.NewGuid().ToString().Replace("-","_"));

            const string fbUserVotes =
                    " CREATE /*TEMPORARY*/ TABLE  IF NOT EXISTS {0}         " +
                    " (                                                     " +
                    "     playlistSongRatingId char(20),                    " +
                    "     Playlist_Id bigint(20),                           " +
                    "     user_Id bigint(20),                               " +
                    "     ratings_Id bigint(20)                             " +
                    " );                                                    ";
            tblBuilder.AppendFormat(fbUserVotes, tempFBUserVotes);

            const string populateFbNewVotes =
                    " TRUNCATE TABLE {0} ;                                  " +
                    " INSERT INTO {0}                                       " +
                    "                                                       " +
                    " SELECT ratings.song_id,                               " +
                    "        ratings.Playlist_Id,                           " +
                    "        fb.userId,                                     " +
                    "        ratings.Id                                     " +
                    " FROM {1} fb                                           " +
                    //commented out - joining by song id, for the chance we'd like to synchoronize all songs
                    //" INNER JOIN playlist_rating ratings                    " +
                    //"     ON songs.id = ratings.Song_id                     " +
                    //current synchronizing by the uinque postid
                    " INNER JOIN playlist_rating ratings                    " +
                    "     ON fb.postId = ratings.FBPostId                   " +
                    " LEFT OUTER JOIN                                       " +
                    "     rating_details details                            " +
                    "     ON details.VotingUser_id = fb.userid              " +
                    "     AND details.songsStatistics_Id = ratings.Id       " +
                    " WHERE                                                 " +
                    "     details.VotingUser_id IS NULL                     " +
                    "     AND fb.isAdmin = 0;                               ";

            tblBuilder.AppendFormat(populateFbNewVotes ,tempFBUserVotes , tempFBWarehouseName);

            const string populateNewUsers =
                " INSERT INTO users (FbId,status, Join_Date, fullName)  " +
                " SELECT DISTINCT fbUserVotes.user_Id as FbId,          " +
                "         'Pending' as status,                          " +
                "         CURDATE() as Join_Date,                       " +
                "         '' as fullName                                " +
                " FROM {0} fbUserVotes                                  " +
                " LEFT OUTER JOIN Users                                 " +
                "     ON Users.FBId = fbUserVotes.user_Id               " +
                " WHERE Users.FBId Is Null;                             ";
            tblBuilder.AppendFormat(populateNewUsers, tempFBUserVotes);

            const string populateRatingsDetails =
                " INSERT INTO rating_details (Rating, LastUpdated, VotingUser_id, songsStatistics_Id)  " +
                " SELECT                                                    " +
                "     1 as rating,                                          " +
                "     CURDATE() as LastUpdated,                             " +
                "     fbUserVotes.user_Id as VotingUser_id,                 " +
                "     ratings_id                                            " +
                " FROM {0} fbUserVotes;                                     ";
            tblBuilder.AppendFormat(populateRatingsDetails, tempFBUserVotes);

            const string updateAdminRating =
                " UPDATE                                                    " +
                "    playlist_rating AS rating                              " +
                "     JOIN (                                                " +
                "        SELECT                                             " +
                "            fb.postId,                                     " +
                "            1 as rating                                    " +
                "        FROM                                               " +
                "            {0} fb                                         " +
                "        WHERE                                              " +
                "            isAdmin = 1                                    " +
                "    ) AS adminInfo ON                                      " +
                "        rating.FBPostId = adminInfo.postId                 " +
                " SET                                                       " +
                "    rating.SummedPositiveRating =                          " +
                "        rating.SummedPositiveRating + adminInfo.rating,    " +
                "     rating.AdminRating = adminInfo.rating                 " +
                "     where rating.AdminRating = 0;                         ";
            tblBuilder.AppendFormat(updateAdminRating, tempFBWarehouseName);

            const string updateSummedPositiveRatings =
            " UPDATE                                                                " +
            "    playlist_rating AS rating                                          " +
            "    LEFT JOIN (                                                        " +
            "        SELECT                                                         " +
            "            details.songsStatistics_Id,                                " +
            "            sum(details.rating) as numOfRatings                        " +
            "        FROM                                                           " +
            "            rating_details details                                     " +
            "        WHERE                                                          " +
            "            details.rating >0                                          " +
            "        GROUP BY                                                       " +
            "            details.songsStatistics_Id                                 " +
            "    ) AS details ON                                                    " +
            "        rating.id = details.songsStatistics_Id                         " +
            " SET                                                                   " +
            "    rating.SummedPositiveRating =                                      " +
            "        CASE rating.AdminRating                                        " +
            "                    WHEN -1 THEN 0                                     " +
            "                    WHEN 0 THEN  0                                     " +
            "                    WHEN 1 THEN  1                                     " +
            "        end                                                            " +
            "        +                                                              " +
            "        CASE                                                           " +
            "            WHEN details.numOfRatings > 0 THEN details.numOfRatings    " +
            "            ELSE 0                                                     " +
            "        END;                                                           ";

            tblBuilder.AppendLine(updateSummedPositiveRatings);

            const string updateNewNumOfVoters =
                " UPDATE                                                                            " +
                "   playlist AS p                                                                   " +
                "   LEFT JOIN (                                                                     " +
                "       SELECT                                                                      " +
                "           ratings.Playlist_id,                                                    " +
                "           CASE when sum(details.rating) is not null  then                         " +
                "                   sum(details.rating)                                             " +
                "           ELSE 0                                                                  " +
                "                                                                                   " +
                "           end                                                                     " +
                "                                                                                   " +
                "           +                                                                       " +
                "                                                                                   " +
                "           sum(CASE ratings.AdminRating                                            " +
                "               when -1 then 1                                                      " +
                "               when 0 then 0                                                       " +
                "               when 1 then 1                                                       " +
                "           end) as numOfRatings                                                    " +
                "       FROM                                                                        " +
                "           rating_details details                                                  " +
                "       RIGHT JOIN playlist_rating ratings                                          " +
                "           ON details.SongsStatistics_id = ratings.Id                              " +
                "      GROUP BY                                                                     " +
                "           ratings.Playlist_id                                                     " +
                "   ) AS ratings ON                                                                 " +
                "        p.id = ratings.Playlist_id                                                 " +
                " SET                                                                               " +
                " NumberOfSongs = (SELECT COUNT(Id) FROM playlist_rating WHERE Playlist_id = p.Id), " +
                " NumberOfVotes =                                                                   " +
                "                                                                                   " +
                " case                                                                              " +
                "     when ratings.numOfRatings > 0 then ratings.numOfRatings                       " +
                "     else NumberOfVotes                                                            " +
                " end;                                                                              ";

            tblBuilder.AppendLine(updateNewNumOfVoters);

            // dropping temp tables
            tblBuilder.AppendFormat(" drop table {0}; ", tempFBWarehouseName);
            tblBuilder.AppendFormat(" drop table {0}; ", tempFBUserVotes);

            tblBuilder.AppendLine(" COMMIT ");
            var command = tblBuilder.ToString();
            try
            {
                var numofRecordsUpdated = Repository.RunBatchSQL(command);
                Logger.InfoFormat("Number of records updated {0}",
                    numofRecordsUpdated);
            }
            catch (Exception ex)
            {
                tblBuilder.Insert(0,
                    "Batch update failed. Offending statment is:\r\n");
                Logger.Fatal(tblBuilder.ToString(), ex);
            }
            tblBuilder.Clear();
        }

        #region Fql Parses
        private List<string> GetLikesFqlRequests(
            IEnumerable<PlaylistSongRatingAndLikes> postsInfo)
        {
            const string likesFqlFormat =
              "SELECT object_id, post_id, user_id from like WHERE {0} limit {1}";

            var distinctFbPostIdRequests = new List<PlaylistSongRatingAndLikes>();
            postsInfo.ToList().ForEach(
                postInfo =>
                {
                    if (!distinctFbPostIdRequests.Any(distinctPostId =>
                            distinctPostId.PlaylistSongRating.FBPostId == postInfo.PlaylistSongRating.FBPostId))
                    {
                        distinctFbPostIdRequests.Add(postInfo);
                    }
                    else
                    {
                        Logger.InfoFormat("FBPostId {0} already exists, skipping PlaylistSongRating {1}",
                            postInfo.PlaylistSongRating.FBPostId, postInfo.PlaylistSongRating.Id);
                    }
                }
                );

            var requestsQuery = new List<string>();
            var requestBuilder = new StringBuilder();

            int currentBatchLikes = 0;
            for (int index = 0; index < distinctFbPostIdRequests.Count; index++)
            {
                var distinctPostInfo = distinctFbPostIdRequests[index];
                
                if (string.IsNullOrEmpty(distinctPostInfo.PlaylistSongRating.FBPostId))
                {
                    continue;
                }

                currentBatchLikes += distinctPostInfo.NumOfLikes;
                NumOfLikes += distinctPostInfo.NumOfLikes;

                requestBuilder.AppendFormat(
                      "post_id = '{0}' or ",
                      distinctPostInfo.PlaylistSongRating.FBPostId);

                /* if too much data in on go or about to exit loop */
                if ((currentBatchLikes + distinctPostInfo.NumOfLikes) >= SINGLE_FQL_QUERY_LIMIT ||
                    index == (distinctFbPostIdRequests.Count - 1))
                {
                    if (requestBuilder.Length >= 3)
                    {
                        // removing trailing or
                        requestBuilder.Length -= 3;
                    }

                    var requestedFbIds = requestBuilder.ToString();

                    var requestSql = string.Format(likesFqlFormat,
                        requestedFbIds, currentBatchLikes);
                    requestsQuery.Add(requestSql);
                    currentBatchLikes = 0;
                    requestBuilder.Clear();
                }
            }
            return requestsQuery;
        }

        private static List<string> GetAggregatedInfoFqlRequests(
            IEnumerable<PlaylistSongRatingAndLikes> postsInfo)
        {
            var fbIds = postsInfo.Select(postInfo => postInfo.PlaylistSongRating)
                                 .Select(playlistSongRating => playlistSongRating.FBPostId)
                                 .Distinct();

            const string queryTemplate =
                "SELECT post_id, likes.count, comments.count FROM stream WHERE {0}";

            var currentBatchLikes = 0;

            var requestBuilder = new StringBuilder();
            var requestsQuery = new List<string>();

            for (var index = 0; index < fbIds.Count(); index++)
            {
                var fbId = fbIds.ElementAt(index);

                if (string.IsNullOrEmpty(fbId))
                {
                    continue;
                }

                requestBuilder.AppendFormat(
                      "post_id = '{0}' or ", fbId);
                currentBatchLikes++;

                /* if too much data in on go or about to exit loop  */
                if ((currentBatchLikes + 1) >= SMALL_FQL_QUERY_LENGTH_LIMIT ||
                    index == (fbIds.Count()- 1))
                {
                    // removing trailing or
                    requestBuilder.Length -= 3;

                    var requestedFbIds = requestBuilder.ToString();

                    var requestSql = string.Format(queryTemplate,
                        requestedFbIds);

                    requestsQuery.Add(requestSql);

                    currentBatchLikes = 0;

                    requestBuilder.Clear();
                }
            }

            return requestsQuery;
        }
        #endregion

        /// <summary>
        /// find the song in the database or create one
        /// </summary>
        ///
        /// <param name="link">
        /// YouTube link
        /// </param>
        ///
        /// <param name="name">
        /// name of the song (YouTube title)
        /// </param>
        ///
        /// <param name="caption">
        /// caption of the post
        /// </param>
        ///
        ///<returns>
        /// either song from the database or a new one (added to the database)
        /// </returns>
        private Song FindOrCreateSong(
            string link,
            string name,
            string caption)
        {
            /* lookup by video ID */
            var songVideoId = FindVideoId(link);
            if (songVideoId == null)
            {
                return null;
            }

            var song = Repository.Query<Song>()
                .Where(s => s.VideoID.Equals(songVideoId))
                .FirstOrDefault();
            if (song != null)
            {
                return song;
            }

            /* look up by artist name and song title */
            var songDetailsNameAttempt = ParseSongNameAndArtist(name);
            var songDetailsCaptionAttempt = ParseSongNameAndArtist(caption);

            var songDetails = new string[2];
            songDetails[0] = string.IsNullOrEmpty(songDetailsNameAttempt[0]) ?
                songDetailsCaptionAttempt[0] : songDetailsNameAttempt[0];

            songDetails[1] = string.IsNullOrEmpty(songDetailsNameAttempt[1]) ?
                songDetailsCaptionAttempt[1] : songDetailsNameAttempt[1];

            /* create new song */
            song = new Song
            {
                ArtistName = songDetails[0],
                Name = songDetails[1],
                LastUpdated = DateTime.Now,
                VideoID = songVideoId
            };
            return song;
        }

        /// <summary>
        /// find the user in the database or create a new one
        /// </summary>
        /// 
        /// <param name="fbidStr">
        /// Facebook Id of the user
        /// </param>
        ///
        /// <returns>
        /// user from the database or newly created and added user
        /// </returns>
        private FbUser FindOrCreateUser(string fbidStr)
        {
            /* parsed facebook ID */
            long fbid;

            if (!long.TryParse(fbidStr, out fbid))
            {
                return null;
            }

            var user = Repository.Get<FbUser>(fbid);
            if (user != null)
            {
                return user;
            }

            var url = string.Format(M_USER_REQUEST_URL, fbid);
            dynamic userObj = FbApp.Get(url);

            /* sanity check */
            if (userObj == null)
            {
                return null;
            }

            user = new FbUser
            {
                Id = fbid,
                FullName = userObj.name,
                Gender = userObj.gender,
                Name = userObj.username,
                LinkToProfile = userObj.link,
                JoinDate = DateTime.Now,
                Status = UserStatus.Pending
            };
            Repository.SaveOrUpdate(user);
            return user;
        }

        /// <summary>
        /// find the YouTube video ID from specified link
        /// </summary>
        ///
        /// <param name="url">
        /// YouTube link to parse
        /// </param>
        ///
        /// <returns>
        /// Video ID if found or null in case of invalid URL
        /// </returns>
        ///
        /// <remarks>
        /// Taken from StackOverflow:
        /// http://stackoverflow.com/questions/3652046/c-regex-to-get-video-id-from-youtube-and-vimeo-by-url
        /// </remarks>
        private static string FindVideoId(string url)
        {
            var youtubeMatch = MYoutubeRex.Match(url);
            if (youtubeMatch.Success)
            {
                return youtubeMatch.Groups[1].Value;
            }
            return null;
        }

        /// <summary>
        /// parse song name from the title
        /// </summary>
        ///
        /// <param name="name">
        /// name as received from YouTube
        /// </param>
        ///
        /// <returns>
        /// array of two strings: fist string is artist name, second name is
        /// the song title
        /// </returns>
        private string[] ParseSongNameAndArtist(string name)
        {
            /* input sanity check */
            if (string.IsNullOrEmpty(name))
            {
                return new[] { string.Empty, string.Empty };
            }
            var split = name.Split(SONG_TITLE_SEPARATORS);
            if (split.Length < 1)
            {
                return new[] { string.Empty, string.Empty };
            }
            if (split.Length < 2)
            {
                return new[] { string.Empty, split[0].Trim() };
            }
            return new[] { split[0].Trim(), split[1].Trim() };
        }

        private void AddNumOfLikesAdded(int numOfLikes)
        {
            LikesRetrieved += numOfLikes;
            LikesRetrivalNotificationHandler(UniqueStatusIndentifier, LikesRetrieved, NumOfLikes);
        }

        public void Dispose()
        {
            try
            {
                Logger.Info("Disposing");

                Repository.CommitTransaction();
                Repository.CloseSession();
            }
            catch
            {
                Repository.RollbackTransaction();
            }
        }

        private class PlaylistSongRatingAndLikes
        {
            public PlaylistSongRating PlaylistSongRating { get; set; }
            public int NumOfLikes { get; set; }
        }
    }
}
