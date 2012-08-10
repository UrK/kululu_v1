using Dror.Common.Data.Contracts;

namespace Kululu.Entities
{
    public class Emails : IEntity
    {
        public virtual string Email { get; set; }

        #region IEntity members
        bool IEntity.IsOwner(object id)
        {
            return false;
        }

        event RepositoryTransactionCompletin IEntity.OnSaving
        {
            add { }
            remove { }
        }

        void IEntity.RiseSaving()
        {
        }
        #endregion
    }
}
