using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections;

namespace Dror.Common.Data.Contracts
{
    public interface IRepository : ISessionManager
    {
        IEntity Get(string entityName, object id);
        
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

        void Query(string sql);

        object RunBatchSQL(string queryString);

        IList<object[]> GropupBy(Type type,
            Type joinType,
            string whereProperty, IEnumerable<IEntity> whereValue, string propertyName, string groupbyPropertyName, string orderByProperyName, int numOfRsls);

        IList<Destination> GetOrderedGrandChildren<Destination>(Type type, string connectedType1,
                        string connectedType2, long id, string orderByProperyName, int startCount, int increment,
                        object orderByValue = null)
            where Destination : IEntity;

        int CountGrandChildren(Type grandchildType, string childProperyName,
                        string fatherPropertyName, long fatherId, string grandchildProperyName, IEntity grandchildProperyValue);

        IEnumerable<Destination> QueryChild<Destination>(Type grandchildType, string fatherProperyName, string motherPropertyName,
                                                         long fatherId, Dictionary<string, string> grandchildProperies, int maxNumRslts);

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
