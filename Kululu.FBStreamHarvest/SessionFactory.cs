using Dror.Common.Data.NHibernate;
using FluentNHibernate.Cfg;
using Kululu.Mappings;
using System.Configuration;
using NHibernate;
using FluentNHibernate.Cfg.Db;

namespace Kululu.FBStreamHarvest
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

        protected new ISessionFactory Factory
        {
            get
            {
                ISessionFactory _factory = null;
                if (_factory == null)
                {
                    _factory = CreateSessionFactory();
                }
                return _factory;
            }
        }

        public new ISession CreateSession()
        {
            var factory = Factory.OpenSession();
            factory.FlushMode = FlushMode.Commit;
            return factory;
        }
    }
}
