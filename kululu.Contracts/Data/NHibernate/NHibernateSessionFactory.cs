using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NHibernate;
using NHib = NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;

namespace Dror.Common.Data.NHibernate
{
    public abstract class NHibernateSessionFactory
    {
        #region Properties

        /// <summary>
        /// Returns the connection string as stored in the web.config file
        /// </summary>
        protected ISessionFactory Factory
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
        private ISessionFactory _factory;

        /// <summary>
        /// Returns the connection string as stored in the web.config file
        /// </summary>
        protected virtual string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[0].ConnectionString;
            }
        }

        #endregion

        #region Public Methods

        public ISession CreateSession()
        {
            var factory = Factory.OpenSession();
            factory.FlushMode = FlushMode.Commit;
            //factory.FlushMode = FlushMode.Always;
            return factory;
        }

        #endregion

        #region For Override

        /// <summary>
        /// Creates the session
        /// </summary>
        /// <returns></returns>
        /// <remarks> Uncomment the relevant lines depending on what you need \ sql server \ mysql
        /// </remarks>
        protected virtual ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                           .Database(CreateDatabaseConfiguration())
                           .Mappings(m => AddMappingsConfiguration(m))
                           .ExposeConfiguration(BuildSchema)
                           .BuildSessionFactory();
        }

        protected virtual IPersistenceConfigurer CreateDatabaseConfiguration()
        {
            return MySQLConfiguration.Standard
                                     .ConnectionString(ConnectionString);
        }

        protected abstract void AddMappingsConfiguration(MappingConfiguration mapping);

        protected virtual void BuildSchema(NHib.Cfg.Configuration config)
        {
            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
                    .Create(false, false);
        }

        #endregion
    }
}
