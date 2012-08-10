using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using System.Linq.Expressions;
using Dror.Common.Data.Contracts;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Impl;
using NHibernate.Engine;
using NHibernate.Loader;
using NHibernate.Loader.Criteria;
using FluentNHibernate.Data;
using NHibernate.Persister.Entity;

namespace Dror.Common.Data.NHibernate
{
    public class NHibernateRepository : IRepository
    {
        #region Properties

        private NHibernateSessionFactory SessionFactory { get; set; }

        private ISession CurrentSession
        {
            get
            {
                if (_session == null)
                {
                    _session = SessionFactory.CreateSession();
                }
                return _session;
            }
        }
        private ISession _session;

        #endregion

        #region Ctor

        public NHibernateRepository(NHibernateSessionFactory sessionFactory)
        {
            if (sessionFactory == null)
                throw new ArgumentNullException("sessionFactory");

            SessionFactory = sessionFactory;
        }

        #endregion

        #region IRepository Methods

        public IEntity Get(string entityName, object id)
        {
            return CurrentSession.Get(entityName, id) as IEntity;
        }

        public T Get<T>(object id) where T : IEntity
        {
             return CurrentSession.Get<T>(id);
        }

        public T GetAndLock<T>(object id) where T : IEntity
        {
            return CurrentSession.Get<T>(id, LockMode.UpgradeNoWait);
        }

        public T GetUnique<T>(Expression<Func<T, bool>> uniqueFilter) where T : IEntity
        {
            return CurrentSession.Query<T>().FirstOrDefault(uniqueFilter);
        }

        public bool Has<T>(Expression<Func<T, bool>> filter) where T : IEntity
        {
            return CurrentSession.Query<T>().Any(filter);
        }

        public int Count<T>(Expression<Func<T, bool>> filter) where T : IEntity
        {
            return CurrentSession.Query<T>().Count(filter);
        }

        public IQueryable<T> Query<T>() where T : IEntity
        {
            return CurrentSession.Query<T>();
        }

        public IQueryable<T> Query<T>(int startIndex, int increment) where T : IEntity
        {
            return CurrentSession.Query<T>()
                          .Skip(startIndex)
                          .Take(increment);
        }

        public IEnumerable<T> Query<T>(string sqlQuery) where T : IEntity
        {
            ISQLQuery qry = CurrentSession.CreateSQLQuery(sqlQuery).AddEntity(typeof(T));
            return qry.List<T>();
        }

        public void Query(string sql)
        {
            var query = CurrentSession.CreateSQLQuery(sql);
            query.ExecuteUpdate();
        }

        public void Save<T>(T entity) where T : IEntity
        {
            entity.RiseSaving();
            CurrentSession.Save(entity);
        }

        public void SaveOrUpdate<T>(T entity) where T : IEntity
        {
            CurrentSession.SaveOrUpdate(entity);
        }

        public void Update<T>(T entity) where T : IEntity
        {
            CurrentSession.Update(entity);
        }

        public void Delete<T>(T entity) where T : IEntity
        {
            CurrentSession.Delete(entity);
        }

        public object RunBatchSQL(string queryString)
        {
            var cmd = CurrentSession.Connection.CreateCommand();
            cmd.CommandText = queryString;
            cmd.CommandTimeout = 120;
            
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// TODO: Refine this method. Check how to propablly perform inner join with other tables
        /// add peroperty names as arrays
        /// </summary>
        /// <param name="type"></param>
        /// <param name="joinType"></param>
        /// <param name="whereProperty"></param>
        /// <param name="whereValue"></param>
        /// <param name="propertyName"></param>
        /// <param name="groupbyPropertyName1"></param>
        /// <param name="groupbyPropertyName2"></param>
        /// <param name="orderByProperyName"></param>
        /// <param name="numOfRsls"></param>
        /// <returns></returns>
        public IList<object[]> GropupBy(Type type, Type joinType, string whereProperty,
               IEnumerable<IEntity> whereValue, string propertyName, string groupbyPropertyName,
               string orderByProperyName, int numOfRsls)
        {
            var criteria = CurrentSession.CreateCriteria(type)
                // SELECT
                                .SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum(propertyName), orderByProperyName)
                // GROUP BY
                               .Add(Projections.GroupProperty(groupbyPropertyName)))
                               .Add(Restrictions.In(whereProperty, whereValue.ToArray()))
            .SetMaxResults(numOfRsls)
            .AddOrder(Order.Desc(orderByProperyName));

            return criteria.List<object[]>();
        }

        public IList<Destination> GetOrderedGrandChildren<Destination>(Type type, string connectedType1, string connectedType2,
                         long id, string orderByProperyName, int startCount, int increment, object orderByValue)
            where Destination : IEntity
        {
            var criteria = CurrentSession.CreateCriteria(type, "grandChild")
                .CreateAlias("Playlist", "child")
                .CreateAlias("child.LocalBusiness", "father")
                .Add(Restrictions.Eq("father.Id", id));

            if (orderByValue!=null)
            {
                criteria.Add(Restrictions.Ge("grandChild." + orderByProperyName, orderByValue));
            }

            criteria.SetFirstResult(startCount)
                .SetMaxResults(increment)
                .AddOrder(Order.Desc("grandChild." + orderByProperyName));

            return criteria.List<Destination>();
        }

        private string GenerateSQL(ICriteria criteria)
        {
            CriteriaImpl criteriaImpl = (CriteriaImpl)criteria;
            ISessionImplementor session = criteriaImpl.Session;
            ISessionFactoryImplementor factory = session.Factory;

            CriteriaQueryTranslator translator =
                new CriteriaQueryTranslator(
                    factory,
                    criteriaImpl,
                    criteriaImpl.EntityOrClassName,
                    CriteriaQueryTranslator.RootSqlAlias);

            String[] implementors = factory.GetImplementors(criteriaImpl.EntityOrClassName);

            CriteriaJoinWalker walker = new CriteriaJoinWalker(
                (IOuterJoinLoadable)factory.GetEntityPersister(implementors[0]),
                                    translator,
                                    factory,
                                    criteriaImpl,
                                    criteriaImpl.EntityOrClassName,
                                    session.EnabledFilters);

            return walker.SqlString.ToString();
        }


        public int CountGrandChildren(Type grandchildType, string childProperyName, string fatherPropertyName, long fatherId, 
                                      string grandchildProperyName, IEntity grandchildProperyValue)
        {
            var criteria = CurrentSession.CreateCriteria(grandchildType, "grandChild")
               .CreateAlias(childProperyName, "child")
               .CreateAlias("child." + fatherPropertyName, "father")
               .Add(Restrictions.Eq("father.Id", fatherId))
               .Add(Restrictions.Eq("grandChild." + grandchildProperyName, grandchildProperyValue))
               .SetProjection(Projections.ProjectionList()
                                         .Add(Projections.Count("grandChild.id")))
               .SetFetchMode(childProperyName, FetchMode.Eager)
               .SetFetchMode("child." + fatherPropertyName, FetchMode.Eager);

            return criteria.UniqueResult<int>();
        }


        public IEnumerable<Destination> QueryChild<Destination>(Type grandchildType, string fatherProperyName,
                                    string motherPropertyName, long fatherId, Dictionary<string, string> grandchildProperies, int maxNumRslts)
        {
            var criteria = CurrentSession.CreateCriteria(grandchildType, "son")
               .CreateAlias(fatherProperyName, "father")
               .CreateAlias(motherPropertyName, "mother")
               .Add(Restrictions.Eq("father.Id", fatherId));

            Disjunction disjunction = Restrictions.Disjunction();
            foreach (var property in grandchildProperies)
            {
                disjunction.Add(Restrictions.Like("mother." + property.Key, string.Format("%{0}%", property.Value)));
            }

            criteria.Add(disjunction);
            criteria.SetFetchMode("son", FetchMode.Eager)
                    .SetFetchMode(motherPropertyName, FetchMode.Eager);
            criteria.SetMaxResults(maxNumRslts);
            
            return criteria.List<Destination>();
        }



        #endregion

        #region ISessionManager Methods

        public void CloseSession()
        {
            CurrentSession.Close();
        }
        
        public bool HasOpenTransaction()
        {
            return CurrentSession.Transaction.IsActive;
        }

        public void BeginTransaction()
        {
            if (!CurrentSession.Transaction.IsActive)
            {
                CurrentSession.Transaction.Begin();
            }
        }

        public void CommitTransaction()
        {
            if (!CurrentSession.Transaction.IsActive)
                return;

            try
            {
                CurrentSession.Transaction.Commit();
                CurrentSession.Flush();
            }
            //TODO: find the base nhibernate exception
            catch (Exception)
            {
                RollbackTransaction();
                throw;
            }
        }

        public void RollbackTransaction()
        {
            if (!CurrentSession.Transaction.IsActive)
                return;

            CurrentSession.Transaction.Rollback();
        }

        #endregion

    }
}
