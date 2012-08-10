using FluentNHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Dror.Common.Data.NHibernate;
using System.Configuration;
using NHib = NHibernate;
using Kululu.Mappings;

namespace Kululu.DBCreator
{
    public class SessionFactory : NHibernateSessionFactory
    {
        #region Properties

        /// <summary>
        /// Returns the connection string as stored in the web.config file
        /// </summary>
        protected override string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["MySql"].ConnectionString;
            }
        }

        #endregion

        protected override void AddMappingsConfiguration(MappingConfiguration m)
        {
            m.FluentMappings.AddFromAssemblyOf<FbUserMapping>();
        }
       
        protected override void BuildSchema(NHib.Cfg.Configuration config)
        {
            new SchemaExport(config)
                  .Create(true, true);
        }
    }
}