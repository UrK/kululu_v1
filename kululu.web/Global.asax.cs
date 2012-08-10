using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kululu.Web.Common;
using Kululu.Web.Startup;
using Kululu.Web.Startup.Binders;

namespace Kululu.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// localization is stolen from these two articles:
    /// http://adamyan.blogspot.com/2010/02/aspnet-mvc-2-localization-complete.html
    /// http://adamyan.blogspot.com/2010/07/addition-to-aspnet-mvc-localization.html
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// register custom error handler
        /// </summary>
        ///
        /// <param name="filters">
        /// collection of existing filters
        /// </param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("resource/{*pathInfo}");

            /* explanation why we are adding this exception, right and to the
             * point!
             * http://weblogs.asp.net/nmarun/archive/2010/03/14/asp-net-mvc-2-throws-exception-for-favicon-ico.aspx
             */
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute(
              "Default",
              "{controller}/{action}/{id}",
              new { controller = "LocalBusiness", action = "Index", id = UrlParameter.Optional }
          );

            routes.MapRoute(
               "Canvas",
               "{controller}/{action}/{name}",
               new { controller = "LocalBusiness", action = "Index", name = string.Empty }
                //new { name  = @"\d+" }
           );



            ModelBinders.Binders.Add(typeof(DateTime),
                new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?),
                new NullableLocalDateTimeBinder());
        }
        

        protected void Application_Start()
        {
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();

            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
            
            var container = HttpContext.Current.Application.GetContainer();
            ControllerBuilder.Current.SetControllerFactory(
                new UnityControllerFactory(container));
        }

        protected void Application_AcquireRequestState(object sender,
            EventArgs e)
        {
            /* It's important to check whether session object is ready */
            if (HttpContext.Current.Session == null)
            {
                return;
            }

            var ci = (CultureInfo)Session[SessionVariables.CULTURE];

            /* Checking first if there is no value in session and set default 
             * language this can happen for first user's request */
            if (ci == null)
            {
                /* Sets default culture to english invariant */
                var langName = Enum.GetName(typeof (Culture), Culture.en);

                /* Try to get values from Accept lang HTTP header */
                if (HttpContext.Current.Request.UserLanguages != null &&
                    HttpContext.Current.Request.UserLanguages.Length != 0)
                {
                    /* Gets accepted list */
                    langName = HttpContext
                        .Current
                        .Request
                        .UserLanguages[0]
                        .Substring(0, 2);
                }
                ci = new CultureInfo(langName);
                Session[SessionVariables.CULTURE] = ci;
            }

            /* Finally setting culture for each request */
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(ci.Name);
        }

        public class CultureConstraint : IRouteConstraint
        {
            private readonly string[] m_values;
            public CultureConstraint(params string[] values)
            {
                m_values = values;
            }

            public bool Match(
                HttpContextBase httpContext,
                Route route,
                string parameterName,
                RouteValueDictionary values,
                RouteDirection routeDirection)
            {

                // Get the value called "parameterName" from the 
                // RouteValueDictionary called "value"
                var value = values[parameterName].ToString();

                // Return true is the list of allowed values contains 
                // this value.
                return Array.Exists(m_values, s => s.Equals(value));
            }
        }

        public enum Culture
        {
            ru = 1,
            he = 2,
            es = 3,
            en = 4
        }
    }
}