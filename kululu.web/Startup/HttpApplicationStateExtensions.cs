using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;

namespace Kululu.Web.Startup
{
    public static class HttpApplicationStateExtensions
    {
        private const string GlobalContainerKey = "UnityContainer";

        public static IUnityContainer GetContainer(this HttpApplicationState application)
        {
            application.Lock();
            try
            {
                IUnityContainer container = application[GlobalContainerKey] as IUnityContainer;
               
                if (container == null)
                {
                    container = new UnityContainer();
                    Initializer.ConfigureUnityContainer(container);
                    application[GlobalContainerKey] = container;
                }
                return container;
            }
            finally
            {
                application.UnLock();
            }
        }
    }
}