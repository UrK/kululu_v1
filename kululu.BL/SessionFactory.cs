using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Kululu.Entities;
using Kululu.BL;

namespace Kululu.ORM
{
    public class SessionFactory : NHibernateSessionFactory
    {
        protected override void AddMappingsConfiguration(MappingConfiguration m)
        {
            m.FluentMappings.AddFromAssemblyOf<Song>();
        }

        protected override string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
        }

        protected override IPersistenceConfigurer CreateDatabaseConfiguration()
        {
            return FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
                                   .ConnectionString(ConnectionString);
        }
    }
}