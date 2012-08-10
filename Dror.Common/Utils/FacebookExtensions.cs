using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook;
using System.Configuration;

namespace Dror.Common.Utils
{
    public static class FacebookExtensions 
    {
        //TODO: upgrade facebook c# sdk

        /// <summary>
        /// Queries facebook with a dictionary of query names and query values
        /// </summary>
        /// <param name="query">query names and query values</param>
        /// <returns></returns>
        public static object Query(this FacebookClient fbApp, Dictionary<string, string> query)
        {
            var parameters = new Dictionary<string, object>();
            parameters["queries"] = query;
            parameters["method"] = "fql.multiquery";

            return fbApp.Get(parameters);
        }

        public static string GetAppSecret(this FacebookClient fbApp)
        {
            /* application settings: AppId */
            var fbSettings = GetAppInfo(fbApp);
            return fbSettings != null ? fbSettings.AppSecret : string.Empty;
        }

        public static string GetAppId(this FacebookClient fbApp)
        {
            /* application settings: AppId */
            var fbSettings = GetAppInfo(fbApp);
            return fbSettings != null ? fbSettings.AppId : string.Empty;
        }

        private static FacebookConfigurationSection GetAppInfo(this FacebookClient fbApp)
        {
            /* application settings: AppId */
            var fbSettings =
                ConfigurationManager.GetSection("facebookSettings")
                as FacebookConfigurationSection;

            return fbSettings;
        }
    }
}
