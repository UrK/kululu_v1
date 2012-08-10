using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using System.Linq.Expressions;
using Dror.Common.Data.Contracts;

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
            return CurrentSession.Linq<T>().FirstOrDefault(uniqueFilter);
        }

        public bool Has<T>(Expression<Func<T, bool>> filter) where T : IEntity
        {
            return CurrentSession.Linq<T>().Any(filter);
        }

        public int Count<T>(Expression<Func<T, bool>> filter) where T : IEntity
        {
            return CurrentSession.Linq<T>().Count(filter);
        }

        public IQueryable<T> Query<T>() where T : IEntity
        {
            return CurrentSession.Linq<T>();
        }

        public IQueryable<T> Query<T>(int startIndex, int increment) where T : IEntity
        {
            return CurrentSession.Linq<T>()
                          .Skip(startIndex)
                          .Take(increment);
        }

        public IEnumerable<T> Query<T>(string sqlQuery) where T : IEntity
        {
            ISQLQuery qry = CurrentSession.CreateSQLQuery(sqlQuery).AddEntity(typeof(T));
            return qry.List<T>();
        }

        public void Save<T>(T entity) where T : IEntity
        {
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
            catch
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
