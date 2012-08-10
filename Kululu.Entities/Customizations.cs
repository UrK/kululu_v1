using Dror.Common.Data.Contracts;

namespace Kululu.Entities
{
    /// <summary>
    /// interface customizations class
    /// </summary>
    public class Customizations : IEntity
    {
        /// <summary>
        /// ID of this record
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// relevant playlist that will be customized
        /// </summary>
        public virtual Playlist Playlist { get; set; }

        /// <summary>
        /// local business customization
        /// </summary>
        public virtual LocalBusiness LocalBusiness { get; set; }

        /// <summary>
        /// locale used by this combination
        /// </summary>
        public virtual string Locale { get; set; }

        bool IEntity.IsOwner(object Id)
        {
            return false;
        }

        public virtual event RepositoryTransactionCompletin OnSaving;

        void IEntity.RiseSaving()
        {
            if (OnSaving != null)
            {
                OnSaving();
            }
        }
    }
}
