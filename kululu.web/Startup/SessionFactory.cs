using NHibernate;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Dror.Common.Data.NHibernate;
using Kululu.Mappings;
using System.Configuration;
using System.Diagnostics;

namespace Kululu.Web.Startup
{
    public class SessionFactory : NHibernateSessionFactory
    {
        protected override void AddMappingsConfiguration(MappingConfiguration m)
        {
            m.FluentMappings.AddFromAssemblyOf<SongMapping>();
        }

        protected override string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["MySql"].ConnectionString;
            }
        }

        protected override ISessionFactory CreateSessionFactory()
        {
            lock (GetType())
            {
                return base.CreateSessionFactory();
            }
        }

        protected override IPersistenceConfigurer CreateDatabaseConfiguration()
        {
            return
                MySQLConfiguration.Standard.ConnectionString(
                    ConnectionString);
        }

        ISessionFactory _factory = null;
        protected new ISessionFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = CreateSessionFactory();
                }
                return _factory;
            }
        }
        
        #region Properties

        #region Public Methods

        public override ISession CreateSession()
        {
            //var interceptor =  new SqlStatementInterceptor();
            //var factory = Factory.OpenSession(interceptor);

            var factory = Factory.OpenSession();
            factory.FlushMode = FlushMode.Commit;
            return factory;
        }

        #endregion
        #endregion

    }

    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            Trace.WriteLine(sql.ToString());
            return sql;
        }
    }
}