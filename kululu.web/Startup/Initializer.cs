using Microsoft.Practices.Unity;
using Dror.Common.Data.Contracts;
using Dror.Common.Data.NHibernate;
using Facebook;
using System.Configuration;
using Dror.Common.Utils.Contracts;
using Dror.Common.Utils.Implementations;
using Dror.Common.Log.Log4Net;
using System.Web;
using Facebook.Web;

namespace Kululu.Web.Startup
{
    public static class Initializer
    {
        public static void ConfigureUnityContainer(IUnityContainer container)
        {
            //Registrations
            #region Repository
            container.RegisterType<NHibernateSessionFactory, SessionFactory>
                                    (new ContainerControlledLifetimeManager());
            var factory = container.Resolve<SessionFactory>();
            var injectedFactory = new InjectionConstructor(factory);
            container.RegisterType<IRepository, NHibernateRepository>
                                    (new ContextLifetimeManager<IRepository>(), injectedFactory);
            #endregion
            
            #region Facebook
            var fbSettings = ConfigurationManager.GetSection("facebookSettings") as FacebookConfigurationSection;
            var injectedFBSettings = new InjectionConstructor(fbSettings.AppId,fbSettings.AppSecret);
            container.RegisterType<FacebookClient, FacebookWebClient>(injectedFBSettings);
            #endregion

            #region Logger
            

            container.RegisterType<Log4NetFactory, LogFactory>
                                    (new ContainerControlledLifetimeManager());
            var logFactory = container.Resolve<LogFactory>();
            

            var injectedLogFactory = new InjectionConstructor(logFactory);
            container.RegisterType<ILogger, Log4Net>
                                    (new ContainerControlledLifetimeManager(),injectedLogFactory);
            #endregion
        }
    }
}
