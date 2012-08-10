using System.Web;
using Dror.Common.Log.Log4Net;

namespace Kululu.Web.Startup
{
    public class LogFactory : Log4NetFactory
    {
        protected override void SetConfigFile()
        {
            string l4net = HttpContext.Current.Server.MapPath("~/web.config");
            //please view this blog post for configurating web.config to save log content to database
            //also note that we only need the following columns: 
            //log_date, thread, log_level, logger, message, exception
            //any other columns included will cause errors in the vanila \ basic version. 
            //same goes with database table

            //http://blogs.lessthandot.com/index.php/WebDev/ServerProgramming/using-postsharp-and-log4net-to-set-up-co
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(l4net));
        }
    }
}