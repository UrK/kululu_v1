using System.Linq;
using Dror.Common.Data.Contracts;
using Facebook;
using System.Web.Mvc;
using Kululu.Entities;
using Kululu.FBStreamHarvest;
using System.Configuration;
using Kululu.Web.Attributes;
using System.Threading.Tasks;
using Kululu.Web.Models.Common;
using Dror.Common.Utils.Contracts;
using System.Collections.Generic;
using System;
using Dror.Common.Utils;

namespace Kululu.Web.Controllers
{
    /// <summary>
    /// controller for Facebook operations
    /// </summary>
    public partial class FbStreamHarvestController : BaseController
    {
        #region Session Utils

        /// <summary>
        /// read the status of the harvester for the context of this session
        /// </summary>
        ///
        /// <param name="id">
        /// id of the harvester
        /// </param>
        ///
        /// <returns>
        /// queue of pending harvester statuses
        /// </returns>
        private Queue<StreamUpdateStatus> GetHarvesterStatus(long id)
        {
            var uniqueContextStore =
                string.Format("HarvesterStatuses-{0}", id);
            if (HttpContext.Application[uniqueContextStore] == null)
            {
                return new Queue<StreamUpdateStatus>();
            }
            return HttpContext.Application[uniqueContextStore]
                as Queue<StreamUpdateStatus>;
        }

        private void SetHarvesterStatus(long id,
            Queue<StreamUpdateStatus> status)
        {
            var uniqueContextStore =
                string.Format("HarvesterStatuses-{0}", id);
            HttpContext.Application[uniqueContextStore] = status;
        }

        private Queue<Tuple<int, int>> GetHarvesterLikesRetrieved(long id)
        {
            var uniqueContextStore =
                string.Format("HarvesterLikesRetrieved-{0}", id);
            if (HttpContext.Application[uniqueContextStore] == null)
            {
                return new Queue<Tuple<int, int>>();
            }
            return HttpContext.Application[uniqueContextStore]
                as Queue<Tuple<int, int>>;
        }

        private void SetHarvesterLikesRetrieved(
            long id, Queue<Tuple<int, int>> likesRetrieved)
        {
            var uniqueContextStore = string.Format("HarvesterLikesRetrieved-{0}", id);
            HttpContext.Application[uniqueContextStore] = likesRetrieved;
        }

        private void UpdateStatus(long id, StreamUpdateStatus status)
        {
            var savedStatus = GetHarvesterStatus(id);
            savedStatus.Enqueue(status);
            SetHarvesterStatus(id, savedStatus);
        }

        private void UpdateLikesRetrival(long id, int completeNumOfLikes, int retrievedLikes)
        {
            var savedLikesRetrieved = GetHarvesterLikesRetrieved(id);
            savedLikesRetrieved.Enqueue(new Tuple<int, int>(completeNumOfLikes, retrievedLikes));
            SetHarvesterLikesRetrieved(id, savedLikesRetrieved);
        }

        #endregion

        private long GetUserUniqueId()
        {
            return (CurrentUser != null) ?
                CurrentUser.Id : new Random().Next(1, 1000);
        }

        public FbStreamHarvestController(
            IRepository repository, FacebookClient fbApp, ILogger logger)
            : base(repository, fbApp, logger)
        {
        }

        public virtual ActionResult Init()
        {
            Session.Remove("Harvester");
            return View(MVC.LocalBusiness.Views.HarvestFacebook);
        }

        public virtual JsonResult Synchronize()
        {
            if (!IsScheduleTask())
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }

            using (var streamRader = GetSocialReader())
            {
                streamRader.Synchronize();
            }

            return BuildSuccessResult(1, null);
        }

        [RequireRequestValueAttribute(new[] { "localBusinessId" })]
        public virtual JsonResult Synchronize(long localBusinessId, short type)
        {
           
            var localBusiness = Repository.Get<LocalBusiness>(localBusinessId);
            var isOwner = MatchUserPrivilege(localBusiness.DefaultPlaylist,
                UserPrivileges.Owner);

            if (!isOwner)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }

            var streamReader = GetSocialReader();

            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, 
                TaskContinuationOptions.LongRunning);
            
            taskFactory.StartNew(() =>
            {
                try
                {
                    streamReader.Synchronize(localBusinessId, (HarvestType)type);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                streamReader.Dispose();
            });

            return BuildSuccessResult(1, null);
        }

        public virtual JsonResult Import()
        {
            /* is user ip in whitelist */
            if (!IsScheduleTask())
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.accessDenied);
            }

            using (var streamRader = GetSocialReader())
            {
                streamRader.Import();
            }

            return BuildSuccessResult(1, null);
        }

        private bool IsScheduleTask()
        {
            return Request.UserHostAddress ==
                ConfigurationManager.AppSettings["ScheduleTaskIPWhiteList"];
        }

        [RequireRequestValueAttribute(new[] { "localBusinessId" })]
        public virtual JsonResult Import(long localBusinessId, short type)
        {
            var localBusiness = Repository.Get<LocalBusiness>(localBusinessId);
            if (localBusiness == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.fanPageNotFound);
            }
            //var isOwner = MatchUserPrivilege(localBusiness.DefaultPlaylist,
            //    UserPrivileges.Owner);
            //if (!isOwner)
            //{
            //    BuildFailureResult(-1,
            //        App_GlobalResources.Errors.accessDenied);
            //}

            var streamReader = GetSocialReader();

            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning,
                TaskContinuationOptions.LongRunning);

            taskFactory.StartNew(() =>
            {
                streamReader.Import(localBusinessId, (HarvestType)type);
                streamReader.Dispose();
            });

            return BuildSuccessResult(1, null);
        }

        private ISocialStreamReader GetSocialReader()
        {
            var filePath = Server.MapPath("~/Files");

            var userlessFBApp = new FacebookClient(
                ConfigurationManager.GetSection("facebookSettings")
                as FacebookConfigurationSection);

            var streamReader = new FacebookStreamReader(
                UpdateStatus,
                UpdateLikesRetrival,
                GetUserUniqueId(),
                filePath,
                Logger,
                userlessFBApp,
                Repository);

            return streamReader;
        }

        /// <summary>
        /// possible statuses of harvester
        /// </summary>
        private static readonly Dictionary<StreamUpdateStatus, string> MStatuses =
            new Dictionary<StreamUpdateStatus, string>
                {
                    {
                        StreamUpdateStatus.Error,
                        App_GlobalResources.HarvesterStatus.error
                    },
                    {
                        StreamUpdateStatus.NotAllowed,
                        App_GlobalResources.HarvesterStatus.notAllowed
                    },
                    {
                        StreamUpdateStatus.Intiailized,
                        App_GlobalResources.HarvesterStatus.initializing
                    },
                    
                    {
                        StreamUpdateStatus.RetrievedPage,
                        App_GlobalResources.HarvesterStatus.retrievedPage
                    },
                    
                    {
                        StreamUpdateStatus.RetrievingPosts,
                        App_GlobalResources.HarvesterStatus.retrievingPosts
                    },
                    
                    {
                        StreamUpdateStatus.RetrievingAggregatedInfo,
                        App_GlobalResources.HarvesterStatus.retrievingAggregatedInfo
                    },
                    
                    {
                        StreamUpdateStatus.RetrievingLikes,
                        App_GlobalResources.HarvesterStatus.retrievingLikes
                    },
                    
                    {
                        StreamUpdateStatus.RetrievingLikesComplete,
                        App_GlobalResources.HarvesterStatus.retrievingLikesComplete
                    },
                    
                    {
                        StreamUpdateStatus.Finished,
                        App_GlobalResources.HarvesterStatus.finished
                    }
                };

        public virtual JsonResult GetImportStatus()
        {
            var harvesterStatus = GetHarvesterStatus(GetUserUniqueId());
            if (harvesterStatus == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.harvesterUndefined);
            }

            if (harvesterStatus.Count == 0)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.harvesterNoNewData);
            }

            // TODO: check if certain status escape, don't get caught
            StreamUpdateStatus currentStatus;
            lock (SessionLock)
            {
                currentStatus = harvesterStatus.Dequeue();
                SetHarvesterStatus(GetUserUniqueId(), harvesterStatus);
            }

            var statusText = MStatuses.ContainsKey(currentStatus) ?
                MStatuses[currentStatus] : string.Empty;

            switch (currentStatus)
            {
                case StreamUpdateStatus.Finished:
                    /* clearing session specific values saved into application
                     * state */
                    SetHarvesterStatus(GetUserUniqueId(), null);
                    SetHarvesterLikesRetrieved(GetUserUniqueId(), null);
                    break;
                case StreamUpdateStatus.Error:
                    SetHarvesterStatus(GetUserUniqueId(), null);
                    SetHarvesterLikesRetrieved(GetUserUniqueId(), null);
                    break;
                default:
                    /* do nothing */
                    break;
            }
            if (currentStatus == StreamUpdateStatus.Finished)
            {
            }

            return BuildSuccessResult(1, new
                {
                    statusKey = currentStatus,
                    statusName = statusText,
                });
        }

        public virtual JsonResult GetImportLikesComplete()
        {
            var likesInfo =
                GetHarvesterLikesRetrieved(GetUserUniqueId()).LastOrDefault();
            if (likesInfo == null)
            {
                return BuildFailureResult(-1,
                    App_GlobalResources.Errors.harvesterNoNewData);
            }

            return BuildSuccessResult(1, new
                {
                    CompleteLikes = likesInfo.Item2,
                    TotalNumOfLikes = likesInfo.Item1
                });
        }
    }
}