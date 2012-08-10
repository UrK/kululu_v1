using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dror.Common.Data.Contracts
{
    public interface IRepository : ISessionManager
    {
        T Get<T>(object id)
            where T : IEntity;

        T GetAndLock<T>(object id)
            where T : IEntity;

        T GetUnique<T>(Expression<Func<T, bool>> uniqueFilter)
            where T : IEntity;

        bool Has<T>(Expression<Func<T, bool>> filter)
            where T : IEntity;

        int Count<T>(Expression<Func<T, bool>> filter)
            where T : IEntity;

        IQueryable<T> Query<T>()
            where T : IEntity;

        IQueryable<T> Query<T>(int startIndex, int increment)
            where T : IEntity;

        IEnumerable<T> Query<T>(string sqlQuery)
            where T : IEntity;

        void Save<T>(T entity)
            where T : IEntity;

        void SaveOrUpdate<T>(T entity)
            where T : IEntity;

        void Update<T>(T entity)
            where T : IEntity;

        void Delete<T>(T entity)
            where T : IEntity;
    }
}
