using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dror.Common.Data.Contracts
{
    public delegate void RepositoryTransactionCompletin();
    public interface IEntity
    {
        bool IsOwner(object Id);
        event RepositoryTransactionCompletin OnSaving;
        void RiseSaving();
    }
}
