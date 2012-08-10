using System;
using System.Collections.Generic;
namespace Kululu.FBStreamHarvest
{
    public delegate void FbStreamUpdate(StreamUpdateStatus status, Exception ex = null);

    public enum StreamUpdateStatus
    {
        Error = -1,
        NotAllowed,
        Intiailized,
        RetrievedPage,
        RetrievingPosts,
        RetrievingAggregatedInfo,
        RetrievingLikes,
        RetrievingLikesComplete,
        Finished
    }

    public enum ContentOrigin
    {
        App,
        Facebook,
        Both
    }

    public enum HarvestType
    {
        NoneSpecified=-1,
        Last24hrs = 0,
        LastWeek = 1,
        All=2,
        TopTen=3
    }
    /// <summary>
    /// interface defining social stream reading methods
    /// </summary>
    public interface ISocialStreamReader : IDisposable
    {
        event FbStreamUpdate FbStreamUpdated;

        IList<StreamUpdateStatus> Status { get; set; }

        int LikesRetrieved { get; set; }

        int NumOfLikes { get; set; }

        /// <summary>
        /// start synchronization of stream
        /// </summary>
        void Synchronize();

        /// <summary>
        /// start synchronization of specified localbusiness stream
        /// </summary>
        void Synchronize(long localBusinessId, HarvestType type);

        /// <summary>
        /// start synchronous stream reading
        /// </summary>
        void Import();

        /// <summary>
        /// start synchronous stream reading of a localBusiness
        /// </summary>
        ///
        /// <param name="localBusinessId">
        /// Id of localbusiness to harvest facebook content
        /// </param>
        ///
        /// <param name="type">
        /// timespane to import
        /// </param>
        void Import(long localBusinessId, HarvestType type);
    }
}
