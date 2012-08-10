using System;
using System.Collections.Generic;
using System.Linq;
using Dror.Common.Data.Contracts;
using Kululu.Entities.Common;

namespace Kululu.Entities
{
    public class LocalBusiness :IEntity
    {
        /// <summary>
        /// URL of the default image of the local business
        /// </summary>
        public const string DEFAULT_IMAGE_URL = "/Content/img/header.png";

        #region Properties
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        
        public virtual long FanPageId { get; set; }
        public virtual string FBFanPageAccessToken { get; set; }

        public virtual string FacebookUrl { get; set; }
        public virtual string ImageUrl { get; set; }

        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime LastModified { get; set; }

        public virtual string Category { get; set; }
        public virtual string AddressStreet { get; set; }
        public virtual string AddressCity { get; set; }

        public virtual string YoutubeChannel { get; set; }
        public virtual bool IsAutomaticallyGenerated { get; set; }
        public virtual bool Approved { get; set; }
        public virtual string Contact { get; set; }

        /// <summary>
        /// email address to which message is sent upon posting song by admin
        /// </summary>
        public virtual string EmailOnAdminPost { get; set; }

        #region Settings

        /// <summary>
        /// are songs added by the user posted to the wall of the fan page?
        /// </summary>
        public virtual bool PublishAdminContentToWall { get; set; }

        /// <summary>
        /// are songs added by the user posted to the wall of the same user?
        /// </summary>
        public virtual bool PublishUserContentToWall { get; set; }

        public virtual bool IsLikeDemanded { get; set; }

        /// <summary>
        /// image shown to the user while he has not liked the page instead of
        /// the playlist
        /// </summary>
        public virtual string LikePageImage { get; set; }

        public virtual int UserPostToWallTypeValue { get; set; }
        public virtual FbPostToWallType UserPostToWallType 
        {
            get
            {
                return (FbPostToWallType)UserPostToWallTypeValue;
            }
            set
            {
                UserPostToWallTypeValue = (int)value;
            }
        }

        #endregion

        public virtual IList<FbUser> Owners { get; set; }
        public virtual IList<Playlist> Playlists { get; set; }
        public virtual Playlist DefaultPlaylist { get; set; }
        public virtual Playlist ImportPlaylist { get; set; }

        #endregion

        #region Ctors
        public LocalBusiness()
        {
            Playlists = new List<Playlist>();
            Owners = new List<FbUser>();
            ImageUrl = DEFAULT_IMAGE_URL;
        }
        #endregion

        #region Methods

        public virtual void AddOwner(FbUser owner)
        {
            if (!Owners.Contains(owner))
            {
                Owners.Add(owner);
                owner.Businesses.Add(this);
            }
        }

        public virtual void RemoveOwner(FbUser owner)
        {
            if (Owners.Contains(owner))
            {
                Owners.Remove(owner);
                owner.Businesses.Remove(this);
            }
        }

        public virtual void AddPlaylist(Playlist playlist, bool replaceDefaultPlaylist = false)
        {
            if (!Playlists.Contains(playlist))
            {
                Playlists.Add(playlist);
                playlist.LocalBusiness = this;
                LastModified = DateTime.Now;
            }

            if (DefaultPlaylist == null || replaceDefaultPlaylist)
            {
                DefaultPlaylist = playlist;
            }
        }

        public virtual void RemovePlaylist(Playlist playlist)
        {
            if (Playlists.Contains(playlist))
            {
                Playlists.Remove(playlist);
                playlist.LocalBusiness = null;
                LastModified = DateTime.Now;
            }

            if (DefaultPlaylist == playlist)
            {
                DefaultPlaylist = Playlists.FirstOrDefault();
            }
        }

        public virtual bool IsOwner(object id)
        {
            return Owners.Any(user => user.Id == (long)id);
        }

        public virtual event RepositoryTransactionCompletin OnSaving;

        public virtual void RiseSaving()
        {
            if (OnSaving != null)
            {
                OnSaving();
            }
        }
        #endregion
    }
}
