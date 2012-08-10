using System;
using System.Web;
using Dror.Common.Data.Contracts;
using System.Web.SessionState;
using Dror.Common.Utils.Contracts;
using System.Web.Script.Serialization;

namespace Kululu.Web.Startup
{
    public class ObjectsPersistance : IHttpModule, IRequiresSessionState
    {
        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            // Below is an example of how you can handle LogRequest event and provide 
            // custom logging implementation for it
            // context.LogRequest += new EventHandler(OnLogRequest);
            context.Error += new EventHandler(context_Error);
            context.BeginRequest += new EventHandler(context_BeginRequest);
            context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
            
            
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            var sessionMaanger = GetSessionManager();
            sessionMaanger.BeginTransaction();
        }

        void context_Error(object sender, EventArgs e)
        {
            //TODO: define some better method to return error code
            var sessionMaanger = GetSessionManager();
            sessionMaanger.RollbackTransaction();

            var ex = HttpContext.Current.Server.GetLastError();

            if (ex != null)
            {
                GetLogger().Fatal(ex);
                WriteError(ex);
            }

            HttpContext.Current.Response.End();
        }

        private void WriteError(Exception ex)
        {
            var serializer = new JavaScriptSerializer();
            //TODO: extract some common class for errors and success messages
            var errorRslt =
                new
                {
                    Success = -1,
                    Message = ex.Message
                };

            HttpContext.Current.Response.Clear();
            if (IsJsonRequest())
            {
                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(serializer.Serialize(errorRslt));
            }
            else
            {
                HttpContext.Current.Response.ContentType = "text/html; charset=utf-8";
                HttpContext.Current.Response.Write(ex.Message);
            }
        }

        private bool IsJsonRequest()
        {
            //TODO: check if this always detects json requests.
            string requestedWith = HttpContext.Current.Request.ServerVariables["HTTP_X_REQUESTED_WITH"] ?? string.Empty;
            return string.Compare(requestedWith, "XMLHttpRequest", true) == 0
                    || HttpContext.Current.Request.ContentType.ToLower().Contains("application/json");
        }

        void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var sessionMaanger = GetSessionManager();
            if (sessionMaanger.HasOpenTransaction())
            {
                sessionMaanger.CommitTransaction();
            }
            
            //refer to blog post about third party cookies http://genotrance.wordpress.com/2006/11/23/session-cookies-rejected-by-internet-explorer/
            //forcing IE to accept third party cookies
            HttpContext.Current.Response.AddHeader("p3p", @"CP=""CURa ADMa DEVa PSAo PSDo OUR BUS UNI PUR INT DEM STA PRE COM NAV OTC NOI DSP COR""");
        }

        private ISessionManager GetSessionManager()
        {
            var container = HttpContext.Current.Application.GetContainer();
            
            IRepository repository = container.Resolve<IRepository>();
            return repository;
        }

        private ILogger GetLogger()
        {
            var container = HttpContext.Current.Application.GetContainer();
            return container.Resolve<ILogger>();
        }

        #endregion

        public void OnLogRequest(Object source, EventArgs e)
        {
            //custom logging logic can go here
        }
    }
}
