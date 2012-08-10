using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dror.Common.Data.Contracts
{
    public interface ISessionManager
    {
        void CloseSession();

        bool HasOpenTransaction();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
